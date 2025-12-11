using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeetLines.Infrastructure.Data;

namespace MeetLines.API.Controllers
{
    /// <summary>
    /// Controlador para recibir webhooks de Telegram Bot API
    /// Similar a WhatsappWebhookController pero adaptado para Telegram
    /// </summary>
    [ApiController]
    public class TelegramWebhookController : ControllerBase
    {
        private readonly MeetLinesPgDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TelegramWebhookController> _logger;

        public TelegramWebhookController(
            MeetLinesPgDbContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<TelegramWebhookController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint que recibe webhooks de Telegram
        /// URL: POST /webhook/telegram/{botToken}
        /// 
        /// FLUJO:
        /// 1. Telegram envía mensajes a esta URL con el botToken en la ruta
        /// 2. Buscamos el proyecto por telegram_bot_token (BYPASS tenant filter)
        /// 3. Si el proyecto tiene telegram_forward_webhook configurado, reenviamos a n8n
        /// 4. n8n procesa el mensaje con el contexto del proyecto
        /// </summary>
        /// <param name="botToken">Token del bot de Telegram (viene en la URL)</param>
        /// <returns>200 OK para acknowledge a Telegram</returns>
        [HttpPost("webhook/telegram/{botToken}")]
        public async Task<IActionResult> Receive([FromRoute] string botToken)
        {
            try
            {
                // Leer el body raw del webhook
                string rawBody;
                using (var reader = new StreamReader(Request.Body))
                {
                    rawBody = await reader.ReadToEndAsync();
                }

                _logger.LogInformation(
                    "📨 Telegram webhook received | Token: {TokenPrefix}... | Body length: {Length} bytes",
                    botToken.Length > 10 ? botToken.Substring(0, 10) : botToken,
                    rawBody.Length
                );

                // 🔍 Buscar proyecto por bot token
                // IMPORTANTE: IgnoreQueryFilters() para BYPASS del filtro de multitenancy
                // porque en este punto no tenemos subdomain y necesitamos buscar por bot_token
                var project = await _context.Projects
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.TelegramBotToken == botToken);

                if (project == null)
                {
                    _logger.LogWarning(
                        "⚠️ No project found for Telegram bot token: {TokenPrefix}...",
                        botToken.Length > 10 ? botToken.Substring(0, 10) : botToken
                    );
                    // Retornamos 200 para evitar que Telegram reintente
                    return Ok(new { status = "ignored", reason = "project_not_found" });
                }

                _logger.LogInformation(
                    "✅ Project found | ID: {ProjectId} | Name: {ProjectName} | Subdomain: {Subdomain}",
                    project.Id,
                    project.Name,
                    project.Subdomain
                );

                // 📤 Reenviar a n8n si está configurado
                if (!string.IsNullOrWhiteSpace(project.TelegramForwardWebhook))
                {
                    var forwardUrl = project.TelegramForwardWebhook;
                    var projectId = project.Id;
                    var projectName = project.Name;

                    // Fire-and-forget: No bloqueamos la respuesta a Telegram
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var client = _httpClientFactory.CreateClient();
                            
                            using var request = new HttpRequestMessage(HttpMethod.Post, forwardUrl)
                            {
                                Content = new StringContent(rawBody, System.Text.Encoding.UTF8, "application/json")
                            };
                            
                            // Agregar headers con metadata del proyecto para n8n
                            request.Headers.Add("X-Project-Id", projectId.ToString());
                            request.Headers.Add("X-Project-Name", projectName);
                            request.Headers.Add("X-Telegram-Bot-Token", botToken);

                            // Timeout de 10 segundos para el forward
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                            var response = await client.SendAsync(request, cts.Token);

                            if (!response.IsSuccessStatusCode)
                            {
                                _logger.LogWarning(
                                    "⚠️ Forward webhook failed | Project: {ProjectId} | Status: {Status} | URL: {Url}",
                                    projectId,
                                    (int)response.StatusCode,
                                    forwardUrl
                                );
                            }
                            else
                            {
                                _logger.LogInformation(
                                    "✅ Webhook forwarded successfully | Project: {ProjectId} | URL: {Url}",
                                    projectId,
                                    forwardUrl
                                );
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            _logger.LogError(
                                "⏱️ Forward webhook timeout | Project: {ProjectId} | URL: {Url}",
                                projectId,
                                forwardUrl
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "❌ Error forwarding webhook | Project: {ProjectId} | URL: {Url}",
                                projectId,
                                forwardUrl
                            );
                        }
                    });

                    _logger.LogInformation(
                        "🚀 Forwarding webhook to n8n | Project: {ProjectId} | URL: {Url}",
                        project.Id,
                        forwardUrl
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "ℹ️ No forward webhook configured | Project: {ProjectId}",
                        project.Id
                    );
                }

                // Responder 200 OK inmediatamente a Telegram
                return Ok(new { status = "received", project_id = project.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing Telegram webhook");
                // Retornamos 200 para evitar reintentos de Telegram
                return Ok(new { status = "error", message = "internal_error" });
            }
        }
    }
}
