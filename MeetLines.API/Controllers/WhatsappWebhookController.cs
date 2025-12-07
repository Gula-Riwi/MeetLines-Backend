using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Domain.Repositories;

namespace MeetLines.API.Controllers
{
    [ApiController]
    public class WhatsappWebhookController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Microsoft.Extensions.Logging.ILogger<WhatsappWebhookController> _logger;

        public WhatsappWebhookController(IProjectRepository projectRepository, IHttpClientFactory httpClientFactory, Microsoft.Extensions.Logging.ILogger<WhatsappWebhookController> logger)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("webhook/whatsapp")]
        [HttpGet("{projectId}/whatsapp")]
        public async Task<IActionResult> Verify([FromRoute(Name = "projectId")] string? projectId,
                                                [FromQuery(Name = "hub.mode")] string? mode,
                                                [FromQuery(Name = "hub.verify_token")] string? verifyToken,
                                                [FromQuery(Name = "hub.challenge")] string? challenge)
        {
            if (!string.Equals(mode, "subscribe", StringComparison.OrdinalIgnoreCase))
                return BadRequest();

            if (string.IsNullOrEmpty(verifyToken))
                return BadRequest();

            var project = await _projectRepository.GetByWhatsappVerifyTokenAsync(verifyToken);
            if (project == null)
                return Forbid();

            return Content(challenge ?? string.Empty, "text/plain");
        }

        [HttpPost("webhook/whatsapp")]
        [HttpPost("{projectId}/whatsapp")]
        public async Task<IActionResult> Receive([FromRoute(Name = "projectId")] string? projectId)
        {
            try
            {
                // Read raw body as string since ASP.NET binding can be finicky with JsonElement
                string rawBody;
                using (var reader = new StreamReader(Request.Body))
                {
                    rawBody = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(rawBody))
                {
                    return BadRequest("Empty body");
                }

                // Parse as JsonElement
                JsonElement body = JsonSerializer.Deserialize<JsonElement>(rawBody);
                
                string phoneNumberId = ExtractPhoneNumberId(body);
                if (string.IsNullOrEmpty(phoneNumberId))
                {
                    _logger.LogWarning("Could not extract phone_number_id from webhook body");
                    return BadRequest("Missing phone_number_id");
                }

                var project = await _projectRepository.GetByWhatsappPhoneNumberIdAsync(phoneNumberId);
                if (project == null)
                {
                    _logger.LogWarning("Project not found for phone_number_id: {PhoneNumberId}", phoneNumberId);
                    return NotFound();
                }

                // If forward webhook is configured for the project, forward the raw payload
                if (!string.IsNullOrWhiteSpace(project.WhatsappForwardWebhook))
                {
                    var client = _httpClientFactory.CreateClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, project.WhatsappForwardWebhook)
                    {
                        Content = new StringContent(rawBody, Encoding.UTF8, "application/json")
                    };
                    request.Headers.Add("X-Project-Id", project.Id.ToString());

                    try
                    {
                        _logger.LogInformation("Forwarding Whatsapp webhook to {Url} for project {ProjectId}", project.WhatsappForwardWebhook, project.Id);
                        var response = await client.SendAsync(request);
                        var responseBody = string.Empty;
                        try
                        {
                            responseBody = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                        }
                        catch (Exception) { /* ignore reading response body errors */ }

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogWarning("Forward webhook returned non-success status {Status} for project {ProjectId}. Response body: {Body}", (int)response.StatusCode, project.Id, responseBody);
                        }
                        else
                        {
                            _logger.LogInformation("Forward webhook delivered successfully for project {ProjectId}. Status: {Status}", project.Id, (int)response.StatusCode);
                        }
                    }
                    catch (Exception exSend)
                    {
                        _logger.LogError(exSend, "Error sending forward webhook to {Url} for project {ProjectId}", project.WhatsappForwardWebhook, project.Id);
                    }
                }

                // TODO: enqueue internal processing if needed

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Whatsapp webhook");
                return StatusCode(500, "Internal server error");
            }
        }

        private string ExtractPhoneNumberId(JsonElement body)
        {
            try
            {
                // Typical payload: entry[0].changes[0].value.metadata.phone_number_id
                if (body.TryGetProperty("entry", out var entry) && entry.GetArrayLength() > 0)
                {
                    var firstEntry = entry[0];
                    if (firstEntry.TryGetProperty("changes", out var changes) && changes.GetArrayLength() > 0)
                    {
                        var firstChange = changes[0];
                        if (firstChange.TryGetProperty("value", out var value))
                        {
                            if (value.TryGetProperty("metadata", out var metadata))
                            {
                                if (metadata.TryGetProperty("phone_number_id", out var phoneIdProp))
                                {
                                    return phoneIdProp.GetString() ?? string.Empty;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore and return empty
            }

            return string.Empty;
        }
    }
}
