using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeetLines.Infrastructure.Data;

namespace MeetLines.API.Controllers
{

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


        [HttpPost("webhook/telegram/{botToken}")]
        public async Task<IActionResult> Receive([FromRoute] string botToken)
        {
            try
            {
 
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

                    return Ok(new { status = "ignored", reason = "project_not_found" });
                }

                _logger.LogInformation(
                    "✅ Project found | ID: {ProjectId} | Name: {ProjectName} | Subdomain: {Subdomain}",
                    project.Id,
                    project.Name,
                    project.Subdomain
                );


                if (!string.IsNullOrWhiteSpace(project.TelegramForwardWebhook))
                {
                    var forwardUrl = project.TelegramForwardWebhook;
                    var projectId = project.Id;
                    var projectName = project.Name;


                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var client = _httpClientFactory.CreateClient();
                            
                            using var request = new HttpRequestMessage(HttpMethod.Post, forwardUrl)
                            {
                                Content = new StringContent(rawBody, System.Text.Encoding.UTF8, "application/json")
                            };
                            

                            request.Headers.Add("X-Project-Id", projectId.ToString());
                            request.Headers.Add("X-Telegram-Bot-Token", botToken);


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
                                "Error forwarding webhook | Project: {ProjectId} | URL: {Url}",
                                projectId,
                                forwardUrl
                            );
                        }
                    });

                    _logger.LogInformation(
                        "Forwarding webhook to n8n | Project: {ProjectId} | URL: {Url}",
                        project.Id,
                        forwardUrl
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "ℹNo forward webhook configured | Project: {ProjectId}",
                        project.Id
                    );
                }


                return Ok(new { status = "received", project_id = project.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Telegram webhook");

                return Ok(new { status = "error", message = "internal_error" });
            }
        }
    }
}
