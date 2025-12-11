using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Application.UseCases.Projects;
using MeetLines.Application.UseCases.Projects.Interfaces;
using MeetLines.Domain.Repositories;
using System.Security.Claims;

namespace MeetLines.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar proyectos/empresas del usuario
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly ICreateProjectUseCase _createProjectUseCase;
        private readonly IGetUserProjectsUseCase _getUserProjectsUseCase;
        private readonly IGetProjectByIdUseCase _getProjectByIdUseCase;
        private readonly IUpdateProjectUseCase _updateProjectUseCase;
        private readonly IDeleteProjectUseCase _deleteProjectUseCase;
        private readonly IConfigureWhatsappUseCase _configureWhatsappUseCase;
        private readonly IConfigureTelegramUseCase _configureTelegramUseCase;

        public ProjectsController(
            ICreateProjectUseCase createProjectUseCase,
            IGetUserProjectsUseCase getUserProjectsUseCase,
            IGetProjectByIdUseCase getProjectByIdUseCase,
            IUpdateProjectUseCase updateProjectUseCase,
            IDeleteProjectUseCase deleteProjectUseCase,
            IConfigureWhatsappUseCase configureWhatsappUseCase,
            IConfigureTelegramUseCase configureTelegramUseCase)
        {
            _createProjectUseCase = createProjectUseCase ?? throw new ArgumentNullException(nameof(createProjectUseCase));
            _getUserProjectsUseCase = getUserProjectsUseCase ?? throw new ArgumentNullException(nameof(getUserProjectsUseCase));
            _getProjectByIdUseCase = getProjectByIdUseCase ?? throw new ArgumentNullException(nameof(getProjectByIdUseCase));
            _updateProjectUseCase = updateProjectUseCase ?? throw new ArgumentNullException(nameof(updateProjectUseCase));
            _deleteProjectUseCase = deleteProjectUseCase ?? throw new ArgumentNullException(nameof(deleteProjectUseCase));
            _configureWhatsappUseCase = configureWhatsappUseCase ?? throw new ArgumentNullException(nameof(configureWhatsappUseCase));
            _configureTelegramUseCase = configureTelegramUseCase ?? throw new ArgumentNullException(nameof(configureTelegramUseCase));
        }

        /// <summary>
        /// Obtiene todos los proyectos del usuario autenticado
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserProjects(CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _getUserProjectsUseCase.ExecuteAsync(userId, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Obtiene un proyecto específico por ID
        /// </summary>
        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProjectById(Guid projectId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _getProjectByIdUseCase.ExecuteAsync(userId, projectId, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Crea un nuevo proyecto/empresa
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _createProjectUseCase.ExecuteAsync(userId, request, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetProjectById), new { projectId = result.Value!.Id }, result.Value);
        }

        /// <summary>
        /// Actualiza un proyecto existente
        /// </summary>
        [HttpPut("{projectId}")]
        public async Task<IActionResult> UpdateProject(Guid projectId, [FromBody] UpdateProjectRequest request, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _updateProjectUseCase.ExecuteAsync(userId, projectId, request, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Elimina un proyecto
        /// </summary>
        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject(Guid projectId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _deleteProjectUseCase.ExecuteAsync(userId, projectId, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }

        /// <summary>
        /// Configura la integración de WhatsApp para un proyecto
        /// </summary>
        [HttpPatch("{projectId}/whatsapp")]
        public async Task<IActionResult> ConfigureWhatsapp(Guid projectId, [FromBody] ConfigureWhatsappRequest request, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _configureWhatsappUseCase.ExecuteAsync(userId, projectId, request, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Obtiene datos públicos de un proyecto específico usando INTEGRATIONS_API_KEY
        /// Este endpoint es usado por n8n para obtener datos del proyecto
        /// GET: api/projects/{projectId}/public
        /// Authorization: Bearer {INTEGRATIONS_API_KEY}
        /// </summary>
        [HttpGet("{projectId}/public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProjectPublicDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProjectPublic(
            [FromRoute] Guid projectId,
            [FromServices] IProjectRepository projectRepository,
            [FromServices] IConfiguration configuration,
            [FromServices] ILogger<ProjectsController> logger,
            CancellationToken ct = default)
        {
            try
            {
                logger.LogInformation("🔍 GetProjectPublic called for ProjectId: {ProjectId}", projectId);
                
                // Validar API Key
                var authHeader = Request.Headers["Authorization"].ToString();
                var apiKey = authHeader.Replace("Bearer ", "").Replace("Bearer", "").Trim();
                var expectedApiKey = configuration["INTEGRATIONS_API_KEY"];

                logger.LogInformation("🔑 API Key present: {HasKey}, Expected key configured: {HasExpected}", 
                    !string.IsNullOrEmpty(apiKey), !string.IsNullOrEmpty(expectedApiKey));

                if (string.IsNullOrEmpty(apiKey) || apiKey != expectedApiKey)
                {
                    logger.LogWarning("⚠️ Invalid API Key for ProjectId: {ProjectId}", projectId);
                    return Unauthorized(new { error = "Invalid API Key" });
                }

                logger.LogInformation("✅ API Key validated, fetching project from database");
                var project = await projectRepository.GetAsync(projectId, ct);
                
                if (project == null)
                {
                    logger.LogWarning("⚠️ Project not found: {ProjectId}", projectId);
                    return NotFound(new { error = "Project not found" });
                }

                logger.LogInformation("✅ Project found: {ProjectName} (ID: {ProjectId})", project.Name, projectId);

                // Devolver solo datos públicos del proyecto
                var dto = new ProjectPublicDto
                {
                    Id = project.Id.ToString(),
                    Name = project.Name,
                    Industry = project.Industry ?? string.Empty,
                    Description = project.Description ?? string.Empty
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error in GetProjectPublic for ProjectId: {ProjectId}", projectId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene credenciales de un proyecto por su phone_number_id de WhatsApp
        /// Este endpoint es usado por n8n para obtener las credenciales dinámicamente
        /// GET: api/projects/phone-number/{phoneNumberId}
        /// </summary>
        [HttpGet("phone-number/{phoneNumberId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProjectCredentialsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByWhatsappPhoneNumber(
            [FromRoute] string phoneNumberId,
            [FromServices] IProjectRepository projectRepository,
            CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumberId))
                {
                    return BadRequest(new { error = "Phone number ID is required" });
                }

                var project = await projectRepository.GetByWhatsappPhoneNumberIdAsync(phoneNumberId, ct);
                if (project == null)
                {
                    return NotFound(new { error = "Project not found" });
                }

                // Devolver solo las credenciales necesarias
                var dto = new ProjectCredentialsDto
                {
                    ProjectId = project.Id.ToString(),
                    ProjectName = project.Name,
                    WhatsappPhoneNumberId = project.WhatsappPhoneNumberId ?? string.Empty,
                    WhatsappAccessToken = project.WhatsappAccessToken ?? string.Empty
                };

                return Ok(dto);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Configura la integración de Telegram para un proyecto
        /// Sigue el mismo patrón que ConfigureWhatsapp
        /// </summary>
        [HttpPatch("{projectId}/telegram")]
        public async Task<IActionResult> ConfigureTelegram(
            Guid projectId,
            [FromBody] ConfigureTelegramRequest request,
            CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _configureTelegramUseCase.ExecuteAsync(userId, projectId, request, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Extrae el ID del usuario del JWT token
        /// </summary>
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID in token");

            return userId;
        }
    }

    public class ProjectCredentialsDto
    {
        public string ProjectId { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string WhatsappPhoneNumberId { get; set; } = null!;
        public string WhatsappAccessToken { get; set; } = null!;
    }
}
