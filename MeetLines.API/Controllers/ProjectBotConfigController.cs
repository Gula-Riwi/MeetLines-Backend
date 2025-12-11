using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.BotSystem;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
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
        private readonly IConfiguration _configuration;
        private readonly IProjectRepository _projectRepository;

        public ProjectBotConfigController(IProjectBotConfigService service, IConfiguration configuration, IProjectRepository projectRepository)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
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
        /// Gets bot configuration for a project
        /// Used by n8n - requires API key authentication
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ProjectBotConfigDto>> GetByProjectId(Guid projectId, CancellationToken ct)
        {
            // Validate API key for n8n integration
            if (!ValidateApiKey())
            {
                return Unauthorized(new { error = "Invalid or missing API key" });
            }

            var config = await _service.GetByProjectIdAsync(projectId, ct);
            if (config == null)
            {
                return NotFound(new { message = "Bot configuration not found" });
            }
            return Ok(config);

        }

        /// <summary>
        /// Gets bot configuration for a project (For authenticated users/dashboard)
        /// Requires JWT Authentication
        /// </summary>
        [HttpGet("my-config")]
        public async Task<ActionResult<ProjectBotConfigDto>> GetMyConfig(Guid projectId, CancellationToken ct)
        {
            try
            {
                // Extract userId from JWT claims
                var userId = Guid.Parse(
                    User.FindFirst("sub")?.Value ?? 
                    User.FindFirst("userId")?.Value ?? 
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                    throw new UnauthorizedAccessException("No user ID found in token"));

                // Validate project ownership
                var isOwner = await _projectRepository.IsUserProjectOwnerAsync(userId, projectId, ct);
                if (!isOwner)
                {
                    return Unauthorized(new { message = "You don't have permission to view bot configuration for this project" });
                }

                var config = await _service.GetByProjectIdAsync(projectId, ct);
                if (config == null)
                {
                    // Return 204 No Content to avoid browser 404 error
                    return NoContent();
                }
                return Ok(config);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid token or user ID" });
            }
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

            try
            {
                // Extract userId from JWT claims - support both standard JWT and .NET claims
                var userId = Guid.Parse(
                    User.FindFirst("sub")?.Value ?? 
                    User.FindFirst("userId")?.Value ?? 
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                    throw new UnauthorizedAccessException("No user ID found in token"));
                
                // Validate project ownership using repository with IgnoreQueryFilters
                var isOwner = await _projectRepository.IsUserProjectOwnerAsync(userId, projectId, ct);
                if (!isOwner)
                {
                    return Unauthorized(new { message = "You don't have permission to create bot configuration for this project" });
                }
                
                var config = await _service.CreateAsync(request, userId, ct);
                return CreatedAtAction(nameof(GetByProjectId), new { projectId }, config);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "You don't have permission to create bot configuration for this project" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create bot configuration", details = ex.Message });
            }
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
            var userId = Guid.Parse(
                User.FindFirst("sub")?.Value ?? 
                User.FindFirst("userId")?.Value ?? 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("No user ID found in token"));
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
