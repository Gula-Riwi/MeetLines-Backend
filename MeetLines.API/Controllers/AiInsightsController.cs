using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Application.DTOs.AiInsights;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/ai-insights")]
    public class AiInsightsController : ControllerBase
    {
        private readonly IAiInsightsService _aiInsightsService;

        public AiInsightsController(IAiInsightsService aiInsightsService)
        {
            _aiInsightsService = aiInsightsService;
        }

        [HttpGet]
        public async Task<ActionResult<AiInsightsDto>> GetInputs(Guid projectId, CancellationToken ct)
        {
            // Optional: Add Project ownership validation here via Authorization filter or Service layer
            var result = await _aiInsightsService.GetProjectInsightsAsync(projectId, ct);
            return Ok(result);
        }
    }
}
