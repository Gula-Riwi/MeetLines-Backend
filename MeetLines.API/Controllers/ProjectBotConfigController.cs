using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.BotSystem;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;
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
                // Extract userId from JWT claims
                var subClaim = User.FindFirst("sub")?.Value;
                var userIdClaim = User.FindFirst("userId")?.Value;
                
                Console.WriteLine($"[BOT-CONFIG] POST Request - ProjectId: {projectId}");
                Console.WriteLine($"[BOT-CONFIG] Claims - sub: {subClaim}, userId: {userIdClaim}");
                Console.WriteLine($"[BOT-CONFIG] All Claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
                
                var userId = Guid.Parse(subClaim ?? userIdClaim ?? throw new UnauthorizedAccessException("No user ID in token"));
                
                Console.WriteLine($"[BOT-CONFIG] Parsed UserId: {userId}");
                
                // Validate project ownership using repository with IgnoreQueryFilters
                var isOwner = await _projectRepository.IsUserProjectOwnerAsync(userId, projectId, ct);
                
                Console.WriteLine($"[BOT-CONFIG] IsUserProjectOwner result: {isOwner}");
                
                if (!isOwner)
                {
                    Console.WriteLine($"[BOT-CONFIG] AUTHORIZATION FAILED - User {userId} is not owner of project {projectId}");
                    return Unauthorized(new { message = "You don't have permission to create bot configuration for this project" });
                }
                
                Console.WriteLine($"[BOT-CONFIG] Authorization successful, creating config...");
                var config = await _service.CreateAsync(request, userId, ct);
                
                Console.WriteLine($"[BOT-CONFIG] Config created successfully with ID: {config.Id}");
                return CreatedAtAction(nameof(GetByProjectId), new { projectId }, config);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[BOT-CONFIG] UnauthorizedAccessException: {ex.Message}");
                return Unauthorized(new { message = "You don't have permission to create bot configuration for this project" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BOT-CONFIG] Exception: {ex.Message}");
                Console.WriteLine($"[BOT-CONFIG] StackTrace: {ex.StackTrace}");
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
