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
    [Route("api/projects/{projectId}/bot-metrics")]
    public class BotMetricsController : ControllerBase
    {
        private readonly IBotMetricsService _service;

        public BotMetricsController(IBotMetricsService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Gets bot metrics with optional date filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetMetrics(
            Guid projectId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? lastNDays = null,
            CancellationToken ct = default)
        {
            var request = new GetMetricsRequest
            {
                ProjectId = projectId,
                StartDate = startDate,
                EndDate = endDate,
                LastNDays = lastNDays
            };

            var metrics = await _service.GetMetricsAsync(request, ct);
            return Ok(metrics);
        }

        /// <summary>
        /// Gets aggregated metrics summary
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<BotMetricsSummaryDto>> GetSummary(
            Guid projectId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken ct = default)
        {
            var summary = await _service.GetSummaryAsync(projectId, startDate, endDate, ct);
            return Ok(summary);
        }

        /// <summary>
        /// Triggers metrics calculation for a specific date (admin/cron)
        /// </summary>
        [HttpPost("calculate")]
        [AllowAnonymous] // cron job
        public async Task<ActionResult<BotMetricsDto>> CalculateMetrics(
            Guid projectId,
            [FromQuery] DateTime? date = null,
            CancellationToken ct = default)
        {
            var targetDate = date ?? DateTime.UtcNow.AddDays(-1).Date;
            var metrics = await _service.UpsertMetricsAsync(projectId, targetDate, ct);
            return Ok(metrics);
        }
    }
}
