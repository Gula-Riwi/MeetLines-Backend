using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MeetLines.Application.DTOs.Config;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class CustomerReactivationService : ICustomerReactivationService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectBotConfigRepository _botConfigRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ICustomerReactivationRepository _reactivationRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CustomerReactivationService> _logger;

        public CustomerReactivationService(
            IProjectRepository projectRepository,
            IProjectBotConfigRepository botConfigRepository,
            IAppointmentRepository appointmentRepository,
            ICustomerReactivationRepository reactivationRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<CustomerReactivationService> logger)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _botConfigRepository = botConfigRepository ?? throw new ArgumentNullException(nameof(botConfigRepository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _reactivationRepository = reactivationRepository ?? throw new ArgumentNullException(nameof(reactivationRepository));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessDailyReactivationsAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Job: Starting Daily Reactivation Process...");
            
            var projects = await _projectRepository.GetAllAsync(ct);

            foreach (var project in projects)
            {
                try
                {
                    await ProcessProjectAsync(project, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Job: Error processing reactivations for Project {project.Id} ({project.Name})");
                }
            }

            _logger.LogInformation("Job: Daily Reactivation Process Completed.");
        }

        private async Task ProcessProjectAsync(Project project, CancellationToken ct)
        {
            // 1. Get Config
            var botConfig = await _botConfigRepository.GetByProjectIdAsync(project.Id);
            if (botConfig == null || string.IsNullOrEmpty(botConfig.ReactivationConfigJson)) return;

            ReactivationConfig config;
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var deserialized = JsonSerializer.Deserialize<ReactivationConfig>(botConfig.ReactivationConfigJson, opts);
                if (deserialized == null) return;
                config = deserialized;
            }
            catch
            {
                // Config invalid / empty
                return; 
            }

            if (config == null || !config.Enabled) return;

            // 2. Identify Inactive Customers
            // Threshold: Last visit < X days ago
            var thresholdDate = DateTimeOffset.UtcNow.AddDays(-config.DelayDays);
            
            var inactiveAppointments = await _appointmentRepository.GetInactiveCustomersAsync(project.Id, thresholdDate, ct);
            if (!inactiveAppointments.Any()) return;

            _logger.LogInformation($"Job: Found {inactiveAppointments.Count()} inactive candidates for Project {project.Name}");

            foreach (var appt in inactiveAppointments)
            {
                // Safety check: Needs phone
                if (string.IsNullOrEmpty(appt.AppUser?.Phone)) continue;

                // 3. Cool-down Check
                // Have we contacted them recently?
                var lastAttempt = await _reactivationRepository.GetLatestByCustomerPhoneAsync(project.Id, appt.AppUser.Phone, ct);
                var attemptNumber = (lastAttempt?.AttemptNumber ?? 0) + 1;

                if (attemptNumber > config.MaxAttempts)
                {
                    // Exceeded max attempts, stop bothering
                    continue;
                }

                if (lastAttempt != null)
                {
                    var daysSinceLastAttempt = (DateTimeOffset.UtcNow - lastAttempt.CreatedAt).TotalDays;
                    if (daysSinceLastAttempt < config.DaysBetweenAttempts)
                    {
                        // Too soon to annoy them again
                        continue;
                    }
                }

                // 4. Select Message Logic
                // If Messages is empty, use default.
                // If Messages has items, cycle through them based on attempt number.
                // Attempt 1 -> Index 0. Attempt 2 -> Index 1...
                
                string messageTemplate;
                if (config.Messages == null || !config.Messages.Any())
                {
                    messageTemplate = "Hola {name}, hace tiempo que no te vemos. ¿Te gustaría agendar una nueva cita?";
                }
                else
                {
                    // Use Modulo to cycle if attempts > messages count, or just clamp to last message?
                    // User probably wants different messages for each stage. Let's clamp to last one if attempts > count.
                    // Or cycle. Let's cycle safely.
                    var index = (attemptNumber - 1) % config.Messages.Count;
                    messageTemplate = config.Messages[index];
                }

                // Prepare final message
                var message = messageTemplate
                    .Replace("{name}", appt.AppUser.FullName ?? "Cliente")
                    .Replace("{days}", (DateTimeOffset.UtcNow - appt.StartTime).Days.ToString());

                // Discount logic
                if (config.OfferDiscount && !string.IsNullOrEmpty(config.DiscountMessage))
                {
                     // Append discount message? Or is it part of the template?
                     // Verify if token {discount} exists in main message. If not, append.
                     var discountMsg = config.DiscountMessage
                        .Replace("{discount}", config.DiscountPercentage.ToString());
                     
                     if (!message.Contains(discountMsg))
                     {
                         message += " " + discountMsg;
                     }
                }

                bool sent = false;
                
                // Use Whatsapp credentials from Project
                if (!string.IsNullOrEmpty(project.WhatsappPhoneNumberId) && !string.IsNullOrEmpty(project.WhatsappAccessToken))
                {
                    sent = await SendWhatsappAsync(project.WhatsappPhoneNumberId, project.WhatsappAccessToken, appt.AppUser.Phone, message);
                }

                if (sent)
                {
                    // 5. Record Attempt
                    // attemptNumber is already calculated above
                    var reactivation = new CustomerReactivation(
                        projectId: project.Id,
                        customerPhone: appt.AppUser.Phone,
                        lastVisitDate: appt.StartTime,
                        daysInactive: (DateTimeOffset.UtcNow - appt.StartTime).Days,
                        attemptNumber: attemptNumber,
                        messageSent: message,
                        customerName: appt.AppUser.FullName,
                        discountOffered: config.OfferDiscount,
                        discountPercentage: config.OfferDiscount ? config.DiscountPercentage : (int?)null
                    );

                    await _reactivationRepository.CreateAsync(reactivation, ct);
                    _logger.LogInformation($"Job: Sent reactivation to {appt.AppUser.Phone} (Attempt {attemptNumber})");
                }
            }
        }

        private async Task<bool> SendWhatsappAsync(string numberId, string token, string toPhone, string text)
        {
            if (string.IsNullOrEmpty(toPhone)) return false;

            try
            {
                // Format phone: needs to ensure country code if missing (assumed 57 for now given context)
                var targetPhone = toPhone.Replace("+", "").Replace(" ", "").Trim();
                if (!targetPhone.StartsWith("57") && targetPhone.Length == 10) targetPhone = "57" + targetPhone;

                var client = _httpClientFactory.CreateClient();
                var url = $"https://graph.facebook.com/v17.0/{numberId}/messages";
                
                var payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = targetPhone,
                    type = "text",
                    text = new { body = text }
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var authHeader = token.StartsWith("Bearer ") ? token : $"Bearer {token}";
                
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                request.Headers.Add("Authorization", authHeader);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Meta API Error (Reactivation): {response.StatusCode} - {err}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception calling Meta API for Reactivation");
                return false;
            }
        }
    }
}
