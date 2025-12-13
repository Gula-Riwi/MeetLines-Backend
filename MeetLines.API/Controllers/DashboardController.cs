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
            [FromQuery] DateTimeOffset? fromDate,
            CancellationToken ct)
        {
            var result = await _getTasksUseCase.ExecuteAsync(projectId, employeeId, fromDate, ct);
            
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }
    }
}
