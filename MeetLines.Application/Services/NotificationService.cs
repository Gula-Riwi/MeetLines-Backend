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
            // AppointmentId is int (from old schema) but repository expects int? Yes.
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
                        message = trans.ReminderMessage
                            .Replace("{name}", appointment.AppUser?.FullName ?? "Cliente")
                            .Replace("{time}", appointment.StartTime.ToOffset(TimeSpan.FromHours(-5)).ToString("hh:mm tt")); // TODO: Use Project timezone
                    }
                }
                catch { /* ignore json error */ }
            }

            // 4. Send WhatsApp
            var project = appointment.Project;
            if (project == null || string.IsNullOrEmpty(project.WhatsappAccessToken) || string.IsNullOrEmpty(project.WhatsappPhoneNumberId))
            {
                _logger.LogWarning($"Job: Project {appointment.ProjectId} missing WhatsApp creds.");
                return;
            }

            // Send Logic
            var success = await SendToMetaAsync(
                project.WhatsappPhoneNumberId, 
                project.WhatsappAccessToken, 
                appointment.AppUser?.Phone, 
                message
            );

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
    }
}
