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
    [Route("api/projects/{projectId}/bot-config")]
    public class ProjectBotConfigController : ControllerBase
    {
        private readonly IProjectBotConfigService _service;

        public ProjectBotConfigController(IProjectBotConfigService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Gets bot configuration for a project
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ProjectBotConfigDto>> GetByProjectId(Guid projectId, CancellationToken ct)
        {
            var config = await _service.GetByProjectIdAsync(projectId, ct);
            if (config == null)
            {
                return NotFound(new { message = "Bot configuration not found" });
            }
            return Ok(config);
        }

        /// <summary>
        /// Creates bot configuration with industry defaults
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProjectBotConfigDto>> Create(
            Guid projectId,
            [FromBody] CreateProjectBotConfigRequest request,
            CancellationToken ct)
        {
            if (request.ProjectId != projectId)
            {
                return BadRequest(new { message = "Project ID mismatch" });
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
            var config = await _service.CreateAsync(request, userId, ct);
            return CreatedAtAction(nameof(GetByProjectId), new { projectId }, config);
        }

        /// <summary>
        /// Updates bot configuration
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<ProjectBotConfigDto>> Update(
            Guid projectId,
            [FromBody] UpdateProjectBotConfigRequest request,
            CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
            var config = await _service.UpdateAsync(projectId, request, userId, ct);
            return Ok(config);
        }

        /// <summary>
        /// Deletes bot configuration
        /// </summary>
        [HttpDelete]
        public async Task<ActionResult> Delete(Guid projectId, CancellationToken ct)
        {
            await _service.DeleteAsync(projectId, ct);
            return NoContent();
        }

        /// <summary>
        /// Gets industry default configuration
        /// </summary>
        [HttpGet("defaults/{industry}")]
        public ActionResult<ProjectBotConfigDto> GetIndustryDefaults(string industry)
        {
            var defaults = _service.GetIndustryDefaults(industry);
            return Ok(defaults);
        }
    }
}
