using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Domain.Repositories;
using System.Net.Http;
using System.Text;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsappController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WhatsappController> _logger;

        public WhatsappController(
            IProjectRepository projectRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<WhatsappController> logger)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Endpoint que recibe los mensajes procesados desde n8n y los envía por WhatsApp
        /// POST: api/whatsapp/send-message
        /// </summary>
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                // Validar project ID del header
                if (!Guid.TryParse(Request.Headers["X-Project-Id"].ToString(), out var projectId))
                {
                    _logger.LogWarning("Invalid or missing X-Project-Id header");
                    return BadRequest(new { error = "Invalid project ID" });
                }

                // Obtener el proyecto
                var project = await _projectRepository.GetAsync(projectId);
                if (project == null)
                {
                    _logger.LogWarning("Project not found: {ProjectId}", projectId);
                    return NotFound(new { error = "Project not found" });
                }

                // Validar que tenga credenciales de WhatsApp
                if (string.IsNullOrWhiteSpace(project.WhatsappAccessToken) || 
                    string.IsNullOrWhiteSpace(project.WhatsappPhoneNumberId))
                {
                    _logger.LogWarning("Project {ProjectId} missing WhatsApp credentials", projectId);
                    return BadRequest(new { error = "WhatsApp credentials not configured" });
                }

                // Enviar mensaje a WhatsApp
                var sent = await SendWhatsAppMessageAsync(
                    project.WhatsappPhoneNumberId,
                    project.WhatsappAccessToken,
                    request.ToPhoneNumber,
                    request.MessageText);

                if (!sent)
                {
                    return StatusCode(500, new { error = "Failed to send WhatsApp message" });
                }

                return Ok(new { success = true, message = "Message sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp message");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private async Task<bool> SendWhatsAppMessageAsync(string phoneNumberId, string accessToken, string toPhoneNumber, string messageText)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"https://graph.instagram.com/v17.0/{phoneNumberId}/messages";

                var payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = toPhoneNumber,
                    type = "text",
                    text = new { body = messageText }
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await client.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("WhatsApp API error: {Status} - {Error}", response.StatusCode, errorContent);
                    return false;
                }

                _logger.LogInformation("WhatsApp message sent successfully to {Phone}", toPhoneNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception sending WhatsApp message");
                return false;
            }
        }
    }

    public class SendMessageRequest
    {
        public string ToPhoneNumber { get; set; } = null!;
        public string MessageText { get; set; } = null!;
    }
}
