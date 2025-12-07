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
    [Route("api/projects/{projectId}/feedback")]
    public class FeedbackController : ControllerBase
    {
        private readonly ICustomerFeedbackService _service;

        public FeedbackController(ICustomerFeedbackService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
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
        /// </summary>
        [HttpPost]
        [AllowAnonymous] // n8n webhook
        public async Task<ActionResult<CustomerFeedbackDto>> Create(
            Guid projectId,
            [FromBody] CreateFeedbackRequest request,
            CancellationToken ct = default)
        {
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
    }
}
