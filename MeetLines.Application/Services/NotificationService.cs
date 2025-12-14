using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IProjectBotConfigRepository _botConfigRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IAppointmentRepository appointmentRepository,
            IProjectBotConfigRepository botConfigRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<NotificationService> logger)
        {
            _appointmentRepository = appointmentRepository;
            _botConfigRepository = botConfigRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task SendAppointmentReminderAsync(int appointmentId)
        {
            // 1. Fetch Appointment with relations (AppUser, Project)
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);

            if (appointment == null) 
            {
                _logger.LogWarning($"Job: Appointment {appointmentId} not found.");
                return;
            }

            // 2. Validate Status
            if (appointment.Status != "confirmed" && appointment.Status != "pending")
            {
                _logger.LogInformation($"Job: Appointment {appointmentId} is {appointment.Status}. Skipping reminder.");
                return;
            }

            if (appointment.ReminderSent)
            {
                _logger.LogInformation($"Job: Reminder already sent for {appointmentId}.");
                return;
            }

            // 3. Get Project Config to customize message
            var botConfig = await _botConfigRepository.GetByProjectIdAsync(appointment.ProjectId);
            
            string message = "Hola, recordamos tu cita pendiente.";
            if (botConfig != null && !string.IsNullOrEmpty(botConfig.TransactionalConfigJson))
            {
                try
                {
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var trans = JsonSerializer.Deserialize<MeetLines.Application.DTOs.Config.TransactionalConfig>(botConfig.TransactionalConfigJson, opts);
                    if (trans != null && !string.IsNullOrEmpty(trans.ReminderMessage))
                    {
                        var culture = new System.Globalization.CultureInfo("es-ES");
                        var timeZoneOffset = TimeSpan.FromHours(-5); // TODO: From Project Timezone
                        
                        var diff = appointment.StartTime - DateTimeOffset.UtcNow;
                        string relative = diff.TotalHours >= 22 ? "mañana" : $"en {Math.Max(1, Math.Round(diff.TotalHours))} horas";
                        if (diff.TotalMinutes < 90) relative = "en breve";

                        message = trans.ReminderMessage
                            .Replace("{name}", appointment.AppUser?.FullName ?? "Cliente")
                            .Replace("{service}", appointment.Service?.Name ?? "Servicio")
                            .Replace("{date}", appointment.StartTime.ToOffset(timeZoneOffset).ToString("d 'de' MMMM", culture))
                            .Replace("{time}", appointment.StartTime.ToOffset(timeZoneOffset).ToString("hh:mm tt", culture))
                            .Replace("{employee}", appointment.Employee?.Name ?? "nuestro equipo")
                            .Replace("{address}", !string.IsNullOrEmpty(appointment.Project?.Address) ? $"📍 {appointment.Project.Address}" : "")
                            .Replace("{company}", appointment.Project?.Name ?? "")
                            .Replace("{relative_time}", relative);
                    }
                }
                catch { /* ignore json error */ }
            }

            // 4. Determine Channel & Send
            var project = appointment.Project;
            bool success = false;
            
            // Channel Detection Strategy:
            // 1. Telegram users (temp) MUST use Telegram.
            // 2. Everyone else (App users, Unified users, WhatsApp users) PREFERS WhatsApp if available in Project.
            // 3. SMS is last resort for App users if Project has no WhatsApp.

            string channel = "whatsapp"; // Priority Default
            var userEmail = appointment.AppUser?.Email ?? "";
            
            if (userEmail.EndsWith("@telegram.temp")) 
            {
                channel = "telegram";
            }
            else 
            {
                // Check if WhatsApp is configured for this project
                bool hasWhatsapp = project != null && !string.IsNullOrEmpty(project.WhatsappPhoneNumberId) && !string.IsNullOrEmpty(project.WhatsappAccessToken);
                
                if (hasWhatsapp)
                {
                    channel = "whatsapp";
                }
                else if (appointment.AppUser?.AuthProvider == "email")
                {
                    channel = "sms";
                }
            }

            if (channel == "telegram")
            {
                 if (!string.IsNullOrEmpty(project?.TelegramBotToken) && !string.IsNullOrEmpty(appointment.AppUser?.Phone))
                 {
                      success = await SendToTelegramAsync(project.TelegramBotToken, appointment.AppUser.Phone, message);
                 }
                 else 
                 {
                      _logger.LogWarning($"Job: Missing Telegram Token or ChatId for Appt {appointmentId}");
                 }
            }
            else if (channel == "sms")
            {
                 // Mock SMS - No Provider Configured Yet
                 _logger.LogInformation($"[SMS SEND MOCK] To: {appointment.AppUser?.Phone} Msg: {message}");
                 success = true; // Assume handled
            }
            else // whatsapp
            {
                 if (project != null && !string.IsNullOrEmpty(project.WhatsappPhoneNumberId))
                 {
                      var targetPhone = appointment.AppUser?.Phone?.Replace("+", "").Replace(" ", "").Trim();
                      if (!string.IsNullOrEmpty(targetPhone) && targetPhone.Length == 10 && !targetPhone.StartsWith("57"))
                      {
                          targetPhone = "57" + targetPhone;
                      }

                      success = await SendToMetaAsync(project.WhatsappPhoneNumberId, project.WhatsappAccessToken!, targetPhone, message);
                 }
                 else
                 {
                      _logger.LogWarning($"Job: Project {appointment.ProjectId} missing WhatsApp creds and SMS fallback failed.");
                 }
            }

            if (success)
            {
                // 5. Mark as Sent
                appointment.MarkReminderAsSent();
                await _appointmentRepository.UpdateAsync(appointment);
                _logger.LogInformation($"Job: Reminder sent for Appointment {appointmentId}");
            }
            else
            {
                _logger.LogError($"Job: Failed to send WhatsApp for Appointment {appointmentId}");
                // Throw to let Hangfire retry
                throw new Exception("Failed to send WhatsApp message via Meta API");
            }
        }

        public async Task SendFeedbackRequestAsync(int appointmentId)
        {
            try 
            {
                // 1. Fetch Appointment
                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
                if (appointment == null || appointment.Status != "confirmed") 
                {
                    _logger.LogWarning($"FeedBackJob: Appointment {appointmentId} (Status: {appointment?.Status}) valid not found or not confirmed.");
                    return;
                }

                // 2. Get Config
                var botConfig = await _botConfigRepository.GetByProjectIdAsync(appointment.ProjectId);
                if (botConfig == null || string.IsNullOrEmpty(botConfig.FeedbackConfigJson))
                {
                    _logger.LogWarning($"FeedBackJob: No Feedback Config found for Project {appointment.ProjectId}");
                    return;
                }

                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var fbConfig = JsonSerializer.Deserialize<MeetLines.Application.DTOs.Config.FeedbackConfig>(botConfig.FeedbackConfigJson, opts);

                if (fbConfig == null || !fbConfig.Enabled)
                {
                    _logger.LogInformation($"FeedBackJob: Feedback disabled for Project {appointment.ProjectId}");
                    return;
                }

                // 3. Prepare Message
                string message = fbConfig.InitialMessage
                    .Replace("{name}", appointment.AppUser?.FullName ?? "Cliente");

                // 4. Send
                var project = appointment.Project;
                if (project != null && !string.IsNullOrEmpty(project.WhatsappPhoneNumberId) && !string.IsNullOrEmpty(project.WhatsappAccessToken))
                {
                    var targetPhone = appointment.AppUser?.Phone?.Replace("+", "").Replace(" ", "").Trim();
                    if (!string.IsNullOrEmpty(targetPhone) && targetPhone.Length == 10 && !targetPhone.StartsWith("57"))
                    {
                        targetPhone = "57" + targetPhone;
                    }

                    var success = await SendToMetaAsync(
                        project.WhatsappPhoneNumberId, 
                        project.WhatsappAccessToken, 
                        targetPhone, 
                        message
                    );

                    if (success) _logger.LogInformation($"FeedBackJob: Feedback request sent for Appt {appointmentId}");
                    else _logger.LogError($"FeedBackJob: Failed to send WhatsApp for Appt {appointmentId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FeedBackJob: Error processing feedback for Appt {appointmentId}");
                throw; // Retry
            }
        }

        private async Task<bool> SendToMetaAsync(string numberId, string token, string? toPhone, string text)
        {
            if (string.IsNullOrEmpty(toPhone)) return false;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"https://graph.facebook.com/v17.0/{numberId}/messages";
                
                var payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = toPhone,
                    type = "text",
                    text = new { body = text }
                };

                var content = new StringContent( JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var authHeader = token.StartsWith("Bearer ") ? token : $"Bearer {token}";
                
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                request.Headers.Add("Authorization", authHeader);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Meta API Error: {response.StatusCode} - {err}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception calling Meta API");
                return false;
            }
        }

        private async Task<bool> SendToTelegramAsync(string botToken, string chatId, string text)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"https://api.telegram.org/bot{botToken}/sendMessage";
                var payload = new { chat_id = chatId, text = text };
                
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                
                if (!response.IsSuccessStatusCode) {
                     var err = await response.Content.ReadAsStringAsync();
                     _logger.LogError($"Telegram Error: {err}");
                     return false;
                }
                return true;
            }
            catch(Exception ex) {
                _logger.LogError(ex, "Telegram Exception");
                return false;
            }
        }
    }
}
