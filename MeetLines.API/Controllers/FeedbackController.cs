using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.BotSystem;
using MeetLines.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration; // Added for API Key access
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId}/feedback")]
    public class FeedbackController : ControllerBase
    {
        private readonly ICustomerFeedbackService _service;
        private readonly IConfiguration _configuration;

        public FeedbackController(ICustomerFeedbackService service, IConfiguration configuration)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Gets all feedback for a project
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAll(
            Guid projectId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var feedbacks = await _service.GetByProjectIdAsync(projectId, page, pageSize, ct);
            return Ok(feedbacks);
        }

        /// <summary>
        /// Gets negative feedback that hasn't been responded to
        /// </summary>
        [HttpGet("negative-unresponded")]
        public async Task<ActionResult> GetNegativeUnresponded(Guid projectId, CancellationToken ct = default)
        {
            var feedbacks = await _service.GetNegativeUnrespondedAsync(projectId, ct);
            return Ok(feedbacks);
        }

        /// <summary>
        /// Creates feedback (called by n8n webhook)
        /// Secured via Manual API Key check + AllowAnonymous to bypass User Auth
        /// </summary>
        [HttpPost]
        [AllowAnonymous] 
        public async Task<ActionResult<CustomerFeedbackDto>> Create(
            Guid projectId,
            [FromBody] CreateFeedbackRequest request,
            CancellationToken ct = default)
        {
            // Security Check
            if (!ValidateApiKey())
            {
                return Unauthorized(new { error = "Unauthorized: Invalid or missing API Key" });
            }

            if (request.ProjectId != projectId)
            {
                return BadRequest(new { message = "Project ID mismatch" });
            }

            var feedback = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetAll), new { projectId }, feedback);
        }

        /// <summary>
        /// Adds owner response to feedback
        /// </summary>
        [HttpPost("{id}/respond")]
        public async Task<ActionResult> AddOwnerResponse(
            Guid projectId,
            Guid id,
            [FromBody] AddOwnerResponseRequest request,
            CancellationToken ct = default)
        {
            await _service.AddOwnerResponseAsync(id, request, ct);
            return Ok();
        }

        /// <summary>
        /// Gets feedback statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<FeedbackStatsDto>> GetStats(Guid projectId, CancellationToken ct = default)
        {
            var stats = await _service.GetStatsAsync(projectId, ct);
            return Ok(stats);
        }

        private bool ValidateApiKey()
        {
            var apiKey = _configuration["INTEGRATIONS_API_KEY"];
            if (string.IsNullOrEmpty(apiKey)) return false; // Fail secure if key not configured

            if (!Request.Headers.TryGetValue("Authorization", out var extractedAuthHeader))
            {
                return false;
            }

            var authHeader = extractedAuthHeader.ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            return token == apiKey;
        }
    }
}
