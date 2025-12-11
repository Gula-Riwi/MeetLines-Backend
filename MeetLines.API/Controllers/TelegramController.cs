using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Domain.Repositories;
using MeetLines.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Microsoft.Extensions.Logging.ILogger<TelegramController> _logger;
        private readonly MeetLines.Infrastructure.Data.MeetLinesPgDbContext _context;

        public TelegramController(
            IProjectRepository projectRepository,
            IHttpClientFactory httpClientFactory,
            Microsoft.Extensions.Logging.ILogger<TelegramController> logger,
            MeetLines.Infrastructure.Data.MeetLinesPgDbContext context)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // --- DEBUG ENDPOINTS (Temporary for Setup) ---

        [HttpGet("debug-projects")]
        public ActionResult DebugListProjects()
        {
             // Returns list of projects to find the ID
             var projects = _context.Projects.IgnoreQueryFilters().ToList();
             return Ok(projects.Select(p => new { p.Id, p.Name, p.Subdomain, p.TelegramBotToken }));
        }



        // --- END DEBUG ---

        /// <summary>
        /// Sends a message via Telegram Bot API using the project's configured bot token.
        /// Used by n8n to send replies.
        /// POST: api/telegram/send-message
        /// Headers: X-Project-Id: {guid}
        /// </summary>
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendTelegramMessageRequest request)
        {
            try
            {
                // 1. Validate Project ID from Header
                if (!Guid.TryParse(Request.Headers["X-Project-Id"].ToString(), out var projectId))
                {
                    _logger.LogWarning("Invalid or missing X-Project-Id header");
                    return BadRequest(new { error = "Invalid or missing X-Project-Id header" });
                }

                // 2. Get Project
                var project = await _projectRepository.GetAsync(projectId);
                if (project == null)
                {
                    _logger.LogWarning("Project not found: {ProjectId}", projectId);
                    return NotFound(new { error = "Project not found" });
                }

                // 3. Validate Telegram Config
                if (string.IsNullOrWhiteSpace(project.TelegramBotToken))
                {
                    _logger.LogWarning("Project {ProjectId} missing Telegram Bot Token", projectId);
                    return BadRequest(new { error = "Telegram integration not configured for this project" });
                }

                // 4. Send Message via Telegram API
                var sent = await SendTelegramApiMessageAsync(
                    project.TelegramBotToken,
                    request.ToChatId,
                    request.MessageText
                );

                if (!sent)
                {
                    return StatusCode(500, new { error = "Failed to send Telegram message" });
                }

                return Ok(new { success = true, message = "Message sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Telegram message");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private async Task<bool> SendTelegramApiMessageAsync(string botToken, string chatId, string text)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                // Telegram Bot API URL
                var url = $"https://api.telegram.org/bot{botToken}/sendMessage";

                var payload = new
                {
                    chat_id = chatId,
                    text = text,
                    parse_mode = "Markdown" 
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Telegram API error: {Status} - {Error}", response.StatusCode, errorContent);
                    return false;
                }

                _logger.LogInformation("Telegram message sent to {ChatId}", chatId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception sending Telegram message to {ChatId}", chatId);
                return false;
            }
        }
    }

    public class SendTelegramMessageRequest
    {
        public string ToChatId { get; set; } = null!;
        public string MessageText { get; set; } = null!;
    }
}
