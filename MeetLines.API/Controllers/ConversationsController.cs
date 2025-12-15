using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.BotSystem;
using MeetLines.Application.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId}/conversations")]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _service;
        private readonly IConfiguration _configuration;

        public ConversationsController(IConversationService service, IConfiguration configuration)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private bool ValidateApiKey()
        {
            var expectedApiKey = _configuration["INTEGRATIONS_API_KEY"];
            if (string.IsNullOrEmpty(expectedApiKey))
            {
                return false;
            }

            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return false;
            }

            var token = authHeader.ToString().Replace("Bearer ", "");
            return token == expectedApiKey;
        }

        /// <summary>
        /// Gets conversations with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAll(
            Guid projectId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? botType = null,
            [FromQuery] bool? requiresHumanAttention = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? assignedToEmployeeId = null,
            CancellationToken ct = default)
        {
            var request = new ConversationListRequest
            {
                ProjectId = projectId,
                Page = page,
                PageSize = pageSize,
                BotType = botType,
                RequiresHumanAttention = requiresHumanAttention,
                AssignedToEmployeeId = assignedToEmployeeId,
                StartDate = startDate,
                EndDate = endDate
            };

            var conversations = await _service.GetByProjectIdAsync(request, ct);
            return Ok(conversations);
        }

        /// <summary>
        /// Gets conversation history for a specific customer
        /// </summary>
        [HttpGet("customer/{customerPhone}")]
        [AllowAnonymous] // n8n webhook
        public async Task<ActionResult> GetByCustomer(Guid projectId, string customerPhone, CancellationToken ct = default)
        {
            // Validate API key for n8n integration
            if (!ValidateApiKey())
            {
                return Unauthorized(new { error = "Invalid or missing API key" });
            }

            var conversations = await _service.GetByCustomerPhoneAsync(projectId, customerPhone, ct);
            return Ok(conversations);
        }

        /// <summary>
        /// Creates a conversation (called by n8n webhook)
        /// Requires API key authentication
        /// </summary>
        [HttpPost]
        [AllowAnonymous] // n8n webhook
        public async Task<ActionResult<ConversationDto>> Create(
            Guid projectId,
            [FromBody] CreateConversationRequest request,
            CancellationToken ct = default)
        {
            // Validate API key for n8n integration
            if (!ValidateApiKey())
            {
                return Unauthorized(new { error = "Invalid or missing API key" });
            }

            if (request.ProjectId != projectId)
            {
                return BadRequest(new { message = "Project ID mismatch" });
            }

            var conversation = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetAll), new { projectId }, conversation);
        }

        /// <summary>
        /// Marks conversation as handled by human
        /// </summary>
        [HttpPost("{id}/mark-handled")]
        public async Task<ActionResult> MarkAsHandled(Guid projectId, Guid id, CancellationToken ct = default)
        {
            var employeeId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
            await _service.MarkAsHandledByHumanAsync(id, employeeId, ct);
            return Ok();
        }

        /// <summary>
        /// Gets average sentiment for conversations
        /// </summary>
        [HttpGet("analytics/sentiment")]
        public async Task<ActionResult> GetAverageSentiment(
            Guid projectId,
            [FromQuery] DateTime? startDate = null,
            CancellationToken ct = default)
        {
            var avgSentiment = await _service.GetAverageSentimentAsync(projectId, startDate, ct);
            return Ok(new { averageSentiment = avgSentiment });
        }

        /// <summary>
        /// Updates conversation metadata (called by n8n Bot 2)
        /// Requires API key authentication
        /// </summary>
        [HttpPatch("{id}")]
        [AllowAnonymous] // n8n webhook
        public async Task<ActionResult> Update(
            Guid projectId,
            Guid id,
            [FromBody] UpdateConversationRequest request,
            CancellationToken ct = default)
        {
            // Validate API key for n8n integration
            if (!ValidateApiKey())
            {
                return Unauthorized(new { error = "Invalid or missing API key" });
            }

            await _service.UpdateAsync(id, request, ct);
            return Ok(new { success = true });
        }

        /// <summary>
        /// Returns conversation control to the bot (creates a reset record)
        /// </summary>
        [HttpPost("phone/{phone}/return-to-bot")]
        public async Task<ActionResult> ReturnToBot(Guid projectId, string phone, CancellationToken ct = default)
        {
            try
            {
                await _service.ReturnToBotAsync(projectId, phone, ct);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = "Internal Server Error in ReturnToBot", 
                    details = ex.Message, 
                    inner = ex.InnerException?.Message 
                });
            }
        }
    }
}
