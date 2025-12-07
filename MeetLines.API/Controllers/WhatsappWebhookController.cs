using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeetLines.Infrastructure.Data;
using MeetLines.Domain.Entities;

namespace MeetLines.API.Controllers
{
    [ApiController]
    public class WhatsappWebhookController : ControllerBase
    {
        private readonly MeetLinesPgDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Microsoft.Extensions.Logging.ILogger<WhatsappWebhookController> _logger;

        public WhatsappWebhookController(
            MeetLinesPgDbContext context,
            IHttpClientFactory httpClientFactory,
            Microsoft.Extensions.Logging.ILogger<WhatsappWebhookController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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

            // BYPASS TENANT FILTER - Webhook público
            var project = await _context.Projects
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.WhatsappVerifyToken == verifyToken);

            if (project == null)
                return Forbid();

            _logger.LogInformation("WhatsApp webhook verified for project {ProjectId}", project.Id);
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

                _logger.LogInformation("Incoming WhatsApp webhook. Body length: {Length}", rawBody.Length);

                // Parse as JsonElement
                JsonElement body = JsonSerializer.Deserialize<JsonElement>(rawBody);
                
                string phoneNumberId = ExtractPhoneNumberId(body);
                Project? project = null;

                if (!string.IsNullOrEmpty(phoneNumberId))
                {
                    // BYPASS TENANT FILTER - Webhook público
                    project = await _context.Projects
                        .AsNoTracking()
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(p => p.WhatsappPhoneNumberId == phoneNumberId);

                    if (project == null)
                    {
                        _logger.LogWarning("Project not found for phone_number_id: {PhoneNumberId}", phoneNumberId);
                    }
                    else
                    {
                        _logger.LogInformation("Project {ProjectId} ({ProjectName}) found for phone_number_id: {PhoneNumberId}", 
                            project.Id, project.Name, phoneNumberId);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not extract phone_number_id from webhook payload");
                }

                // If we didn't find project by phone number, allow specifying project via header X-Project-Id
                if (project == null)
                {
                    if (Request.Headers.TryGetValue("X-Project-Id", out var projectIdHeader))
                    {
                        if (Guid.TryParse(projectIdHeader.ToString(), out var projectGuid))
                        {
                            // BYPASS TENANT FILTER
                            project = await _context.Projects
                                .AsNoTracking()
                                .IgnoreQueryFilters()
                                .FirstOrDefaultAsync(p => p.Id == projectGuid);

                            if (project != null)
                            {
                                _logger.LogInformation("Project {ProjectId} found via X-Project-Id header", project.Id);
                            }
                        }
                    }
                }

                if (project == null)
                {
                    _logger.LogWarning("Project not resolved for incoming Whatsapp webhook. phone_number_id={PhoneNumberId}, headerXProjectId={Header}", 
                        phoneNumberId, Request.Headers["X-Project-Id"].ToString());
                    // Acknowledge the webhook even if we can't resolve the project to avoid retries from the provider
                    return Ok();
                }

                // If forward webhook is configured for the project, forward the raw payload in background (fire-and-forget)
                if (!string.IsNullOrWhiteSpace(project.WhatsappForwardWebhook))
                {
                    var forwardUrl = project.WhatsappForwardWebhook;
                    var projectIdForHeader = project.Id.ToString();
                    var raw = rawBody;

                    _logger.LogInformation("Scheduling forward of Whatsapp webhook to {Url} for project {ProjectId}", forwardUrl, project.Id);

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var client = _httpClientFactory.CreateClient();
                            using var request = new HttpRequestMessage(HttpMethod.Post, forwardUrl)
                            {
                                Content = new StringContent(raw, Encoding.UTF8, "application/json")
                            };
                            request.Headers.Add("X-Project-Id", projectIdForHeader);

                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                            var response = await client.SendAsync(request, cts.Token);
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
                            _logger.LogError(exSend, "Error sending forward webhook to {Url} for project {ProjectId}", forwardUrl, project.Id);
                        }
                    });
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
                // If body itself is a primitive (string/number) containing the phone id
                if (body.ValueKind == JsonValueKind.String)
                {
                    var s = body.GetString();
                    if (!string.IsNullOrWhiteSpace(s)) return s;
                }
                else if (body.ValueKind == JsonValueKind.Number)
                {
                    return body.GetRawText();
                }

                // Direct root property: { "phone_number_id": "..." }
                if (body.TryGetProperty("phone_number_id", out var rootPhone))
                {
                    if (rootPhone.ValueKind == JsonValueKind.String)
                        return rootPhone.GetString() ?? string.Empty;
                    return rootPhone.GetRawText();
                }

                if (body.TryGetProperty("value", out var directValue))
                {
                    // value might be an object or a JSON-encoded string
                    if (directValue.ValueKind == JsonValueKind.String)
                    {
                        var inner = directValue.GetString();
                        if (!string.IsNullOrWhiteSpace(inner))
                        {
                            try
                            {
                                var parsed = JsonSerializer.Deserialize<JsonElement>(inner);
                                var fromInner = ExtractPhoneNumberId(parsed);
                                if (!string.IsNullOrEmpty(fromInner)) return fromInner;
                            }
                            catch { /* ignore */ }
                        }
                    }
                    else if (directValue.ValueKind == JsonValueKind.Object)
                    {
                        if (directValue.TryGetProperty("metadata", out var meta) && meta.TryGetProperty("phone_number_id", out var metaPhone))
                        {
                            if (metaPhone.ValueKind == JsonValueKind.String)
                                return metaPhone.GetString() ?? string.Empty;
                            return metaPhone.GetRawText();
                        }
                    }
                }

                if (body.TryGetProperty("entry", out var entry) && entry.GetArrayLength() > 0)
                {
                    var firstEntry = entry[0];
                    if (firstEntry.TryGetProperty("changes", out var changes) && changes.GetArrayLength() > 0)
                    {
                        var firstChange = changes[0];
                        if (firstChange.TryGetProperty("value", out var value))
                        {
                            // value might be object or stringified JSON
                            if (value.ValueKind == JsonValueKind.String)
                            {
                                var inner = value.GetString();
                                if (!string.IsNullOrWhiteSpace(inner))
                                {
                                    try
                                    {
                                        var parsed = JsonSerializer.Deserialize<JsonElement>(inner);
                                        var fromInner = ExtractPhoneNumberId(parsed);
                                        if (!string.IsNullOrEmpty(fromInner)) return fromInner;
                                    }
                                    catch { /* ignore */ }
                                }
                            }
                            else if (value.ValueKind == JsonValueKind.Object)
                            {
                                if (value.TryGetProperty("metadata", out var metadata))
                                {
                                    if (metadata.TryGetProperty("phone_number_id", out var phoneIdProp))
                                    {
                                        if (phoneIdProp.ValueKind == JsonValueKind.String)
                                            return phoneIdProp.GetString() ?? string.Empty;
                                        return phoneIdProp.GetRawText();
                                    }
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
