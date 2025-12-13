using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.UseCases.Dashboard;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly GetDashboardStatsUseCase _getStatsUseCase;
        private readonly GetDashboardTasksUseCase _getTasksUseCase;

        public DashboardController(
            GetDashboardStatsUseCase getStatsUseCase, 
            GetDashboardTasksUseCase getTasksUseCase)
        {
            _getStatsUseCase = getStatsUseCase ?? throw new ArgumentNullException(nameof(getStatsUseCase));
            _getTasksUseCase = getTasksUseCase ?? throw new ArgumentNullException(nameof(getTasksUseCase));
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(Guid projectId, CancellationToken ct)
        {
            // Optional: proper access control verification (Is user allowed for this project?)
            // Usually done via middleware or querying "UserProject" table.
            // For now, relying on Authorization + valid ProjectId.
            // TODO: Ensure user owns or is member of project.
            
            var result = await _getStatsUseCase.ExecuteAsync(projectId, ct);
            
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasks(
            Guid projectId,
            [FromQuery] Guid? employeeId, 
            [FromQuery] string? period, // "today", "week", "month"
            CancellationToken ct)
        {
            DateTimeOffset? fromDate = null;
            DateTimeOffset? toDate = null;
            var now = DateTimeOffset.UtcNow;

            // Calculate date range based on period
            if (!string.IsNullOrEmpty(period))
            {
                switch (period.ToLower())
                {
                    case "today":
                        // Start of today:
                        fromDate = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
                        // End of today:
                        toDate = fromDate.Value.AddDays(1).AddTicks(-1);
                        break;
                    case "week":
                        // Start of week (assuming Monday start? Or Sunday? culture independent usually means Monday for business)
                        // Let's use simple logic: Last 7 days? Or Current Calendar Week?
                        // User said "filtrar por semana". Usually means "This week".
                        // Logic: Find start of week.
                        var diff = now.DayOfWeek - DayOfWeek.Monday;
                        if (diff < 0) diff += 7;
                        fromDate = new DateTimeOffset(now.Date.AddDays(-diff), TimeSpan.Zero); // Monday
                        toDate = fromDate.Value.AddDays(7).AddTicks(-1); // Sunday end
                        break;
                    case "month":
                        // Start of month
                        fromDate = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        // End of month
                        toDate = fromDate.Value.AddMonths(1).AddTicks(-1);
                        break;
                    default:
                        // Fallback to default (last 30 days logic inside UseCase if null)
                        break; 
                }
            }

            var result = await _getTasksUseCase.ExecuteAsync(projectId, employeeId, fromDate, toDate, ct);
            
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpPatch("tasks/{taskId}/status")]
        public async Task<IActionResult> UpdateTaskStatus(
            Guid projectId, 
            int taskId, 
            [FromBody] MeetLines.Application.DTOs.Dashboard.UpdateTaskStatusRequest request,
            [FromServices] UpdateTaskStatusUseCase updateTaskStatusUseCase,
            CancellationToken ct)
        {
            var result = await updateTaskStatusUseCase.ExecuteAsync(projectId, taskId, request, ct);

            if (!result.IsSuccess) return BadRequest(new { error = result.Error });

            return NoContent();
        }
    }
}
