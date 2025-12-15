using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;
using MeetLines.Domain.Entities;

namespace MeetLines.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IProjectBotConfigRepository _botConfigRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NotificationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConversationRepository _conversationRepository;

        public NotificationService(
            IAppointmentRepository appointmentRepository,
            IProjectBotConfigRepository botConfigRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<NotificationService> logger,
            IConfiguration configuration,
            IConversationRepository conversationRepository)
        {
            _appointmentRepository = appointmentRepository;
            _botConfigRepository = botConfigRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
            _conversationRepository = conversationRepository;
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
                if (appointment == null) return;
                
                // Allow 'confirmed' or 'completed'. Skip if 'cancelled'
                if (appointment.Status == "cancelled" || appointment.Status == "pending") 
                {
                    _logger.LogWarning($"FeedBackJob: Appt {appointmentId} is {appointment.Status}. Skipping.");
                    return;
                }

                // 2. Get Config (Check if Enabled)
                var botConfig = await _botConfigRepository.GetByProjectIdAsync(appointment.ProjectId);
                if (botConfig == null || string.IsNullOrEmpty(botConfig.FeedbackConfigJson)) return;

                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var fbConfig = JsonSerializer.Deserialize<MeetLines.Application.DTOs.Config.FeedbackConfig>(botConfig.FeedbackConfigJson, opts);

                if (fbConfig == null || !fbConfig.Enabled) 
                {
                    _logger.LogInformation($"FeedBackJob: Feedback disabled for Project {appointment.ProjectId}");
                    return;
                }

                // 3. Prepare Message with Variables (Moved UP)
                var message = fbConfig.RequestMessage ?? "Hola {customerName}, ¿cómo calificarías tu experiencia del 1 al 5?";
                var culture = new System.Globalization.CultureInfo("es-ES");
                var timeZoneOffset = TimeSpan.FromHours(-5); // Fix: Use Project Timezone later

                message = message
                    .Replace("{customerName}", appointment.AppUser?.FullName ?? "Cliente")
                    .Replace("{name}", appointment.AppUser?.FullName ?? "Cliente")
                    .Replace("{service}", appointment.Service?.Name ?? "Servicio")
                    .Replace("{date}", appointment.StartTime.ToOffset(timeZoneOffset).ToString("d 'de' MMMM", culture))
                    .Replace("{time}", appointment.StartTime.ToOffset(timeZoneOffset).ToString("hh:mm tt", culture))
                    .Replace("{employee}", appointment.Employee?.Name ?? "nuestro equipo");

                // 4. Set Conversation State (DB) - Now uses 'message'
                if (!string.IsNullOrEmpty(appointment.AppUser?.Phone))
                {
                    var conversationState = new Conversation(
                        projectId: appointment.ProjectId,
                        customerPhone: appointment.AppUser.Phone,
                        customerMessage: "(System Trigger)", 
                        botResponse: message, 
                        botType: "feedback_wait", 
                        customerName: appointment.AppUser.FullName
                    );
                    await _conversationRepository.CreateAsync(conversationState);
                }

                // 5. Trigger Webhook
                var webhookUrl = appointment.Project?.WhatsappForwardWebhook;
                if (string.IsNullOrEmpty(webhookUrl))
                {
                    _logger.LogWarning($"FeedBackJob: Project {appointment.ProjectId} has no WhatsappForwardWebhook configured.");
                    return;
                }

                var payload = new 
                {
                     type = "feedback_request",
                     projectId = appointment.ProjectId,
                     appointmentId = appointment.Id,
                     clientName = appointment.AppUser?.FullName,
                     clientPhone = appointment.AppUser?.Phone,
                     employeeName = appointment.Employee?.Name,
                     serviceName = appointment.Service?.Name,
                     date = appointment.StartTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                     ratingMessage = message 
                };
                 
                var client = _httpClientFactory.CreateClient();
                // Auth Header Removed by User Request
                
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(webhookUrl, content);
                 
                if (response.IsSuccessStatusCode)
                     _logger.LogInformation($"FeedBackJob: Webhook triggered successfully for Appt {appointmentId} to {webhookUrl}");
                else
                     _logger.LogError($"FeedBackJob: Webhook failed {response.StatusCode} response: {await response.Content.ReadAsStringAsync()}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FeedBackJob: Error for Appt {appointmentId}");
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
        public async Task NotifyEmployeeOfNewChatAsync(Guid projectId, Guid employeeId, string customerPhone, CancellationToken ct = default)
        {
            try
            {
                // In V1, we just log it or maybe send email if Email is configured.
                // Ideally this sends a WhatsApp Template to the Employee's phone.
                _logger.LogInformation("🔔 NOTIFICATION: Employee {EmployeeId} assigned to chat with {CustomerPhone}", employeeId, customerPhone);
                
                // TODO: Retrieve employee email/phone and send actual alert
                // For now, this placeholder ensures the service call succeeds.
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify employee of new chat.");
            }
        }

        public async Task SendNegativeFeedbackAlertAsync(Guid projectId, string message, CancellationToken ct = default)
        {
            try
            {
                // V1: Log the alert. Ideally this sends an email or WhatsApp to the Project Owner.
                _logger.LogWarning("🚨 NEGATIVE FEEDBACK ALERT for Project {ProjectId}: {Message}", projectId, message);
                
                // Placeholder for actual sending logic
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send negative feedback alert.");
            }
        }
    }
}
