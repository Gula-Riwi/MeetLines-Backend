using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Application.UseCases.Projects.Interfaces;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Use case para configurar la integración de Telegram en un proyecto existente
    /// Sigue el mismo patrón que ConfigureWhatsappUseCase
    /// </summary>
    public class ConfigureTelegramUseCase : IConfigureTelegramUseCase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfigureTelegramUseCase(
            IProjectRepository projectRepository,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            Guid projectId,
            ConfigureTelegramRequest request,
            CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return Result<ProjectResponse>.Fail("User ID is invalid");

            var project = await _projectRepository.GetAsync(projectId, ct);
            if (project == null)
                return Result<ProjectResponse>.Fail("Project not found");

            if (project.UserId != userId)
                return Result<ProjectResponse>.Fail("Unauthorized access to project");

            // 1️⃣ Construir URL del webhook para Telegram
            // Usar la URL de n8n con el botToken como parámetro para que n8n pueda identificar el proyecto
            string webhookUrl;
            if (!string.IsNullOrWhiteSpace(request.CustomWebhookUrl))
            {
                webhookUrl = request.CustomWebhookUrl;
                // Si no incluye el token, agregarlo (Telegram requiere el token en la URL si usas nuestro Controller, pero si es custom total, asumimos que está bien)
                // En nuestro caso: /webhook/telegram/{botToken}
                if (!webhookUrl.Contains(request.BotToken))
                {
                   webhookUrl = $"{webhookUrl.TrimEnd('/')}/webhook/telegram/{request.BotToken}";
                }
            }
            else
            {
                // Construir URL apuntando al BACKEND (TelegramWebhookController)
                // Se asume que existe una config "Global:ApiBaseUrl" o se construye
                var apiBaseUrl = _configuration["TELEGRAM_WEBHOOK_BASE"]
                                 ?? _configuration["Global:ApiBaseUrl"] 
                                 ?? _configuration["Multitenancy:ApiUrl"]
                                 ?? "https://services.meet-lines.com";
                                 
                webhookUrl = $"{apiBaseUrl.TrimEnd('/')}/webhook/telegram/{request.BotToken}";
            }

            // 2️⃣ Configurar webhook en Telegram API
            try
            {
                var client = _httpClientFactory.CreateClient();
                var telegramApiUrl = $"https://api.telegram.org/bot{request.BotToken}/setWebhook";
                
                var telegramRequest = new { url = webhookUrl };
                var response = await client.PostAsJsonAsync(telegramApiUrl, telegramRequest, ct);
                var responseBody = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    return Result<ProjectResponse>.Fail(
                        $"Failed to configure Telegram webhook: {responseBody}. Verify bot token is valid.");
                }
            }
            catch (Exception ex)
            {
                return Result<ProjectResponse>.Fail(
                    $"Error calling Telegram API: {ex.Message}");
            }

            // 3️⃣ Obtener forward webhook de configuración o del request
            var forwardWebhook = request.ForwardWebhook;
            if (string.IsNullOrWhiteSpace(forwardWebhook))
            {
                // Por defecto desde configuración (similar a WhatsApp)
                forwardWebhook = _configuration["TELEGRAM_FORWARD_WEBHOOK"] 
                    ?? _configuration["Webhooks:N8nBaseUrl"] 
                    ?? "https://services.meet-lines.com/webhook/telegram-bot";
            }

            // 4️⃣ Actualizar proyecto con los datos de Telegram
            project.UpdateTelegramIntegration(
                request.BotToken,
                request.BotUsername,
                forwardWebhook
            );

            await _projectRepository.UpdateAsync(project, ct);

            return Result<ProjectResponse>.Ok(MapToResponse(project));
        }

        private ProjectResponse MapToResponse(Domain.Entities.Project project)
        {
            var baseDomain = _configuration["Multitenancy:BaseDomain"] ?? "meet-lines.com";
            var protocol = _configuration["Multitenancy:Protocol"] ?? "https";
            var fullUrl = $"{protocol}://{project.Subdomain}.{baseDomain}";

            return new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Subdomain = project.Subdomain,
                FullUrl = fullUrl,
                Industry = project.Industry,
                Description = project.Description,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                WhatsappPhoneNumberId = project.WhatsappPhoneNumberId,
                WhatsappForwardWebhook = project.WhatsappForwardWebhook,
                TelegramBotToken = project.TelegramBotToken,
                TelegramBotUsername = project.TelegramBotUsername,
                TelegramForwardWebhook = project.TelegramForwardWebhook
            };
        }
    }
}
