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
            try
            {
                var result = await _aiInsightsService.GetProjectInsightsAsync(projectId, ct);
                return Ok(new MeetLines.API.DTOs.ApiResponse<AiInsightsDto> 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new MeetLines.API.DTOs.ApiResponse 
                { 
                    Success = false, 
                    Message = $"Internal Server Error: {ex.Message}" 
                });
            }
        }
    }
}
