using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Application.UseCases.Projects;
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
        private readonly IGetPublicProjectsUseCase _getPublicProjectsUseCase;
        private readonly IGetPublicProjectEmployeesUseCase _getPublicProjectEmployeesUseCase;
        private readonly IUploadProjectPhotoUseCase _uploadProjectPhotoUseCase;
        private readonly IConfiguration _configuration;

        public ProjectsController(
            ICreateProjectUseCase createProjectUseCase,
            IGetUserProjectsUseCase getUserProjectsUseCase,
            IGetProjectByIdUseCase getProjectByIdUseCase,
            IUpdateProjectUseCase updateProjectUseCase,
            IDeleteProjectUseCase deleteProjectUseCase,
            IConfigureWhatsappUseCase configureWhatsappUseCase,
            IGetPublicProjectsUseCase getPublicProjectsUseCase,
            IGetPublicProjectEmployeesUseCase getPublicProjectEmployeesUseCase,
            IUploadProjectPhotoUseCase uploadProjectPhotoUseCase,
            IConfiguration configuration)
        {
            _createProjectUseCase = createProjectUseCase ?? throw new ArgumentNullException(nameof(createProjectUseCase));
            _getUserProjectsUseCase = getUserProjectsUseCase ?? throw new ArgumentNullException(nameof(getUserProjectsUseCase));
            _getProjectByIdUseCase = getProjectByIdUseCase ?? throw new ArgumentNullException(nameof(getProjectByIdUseCase));
            _updateProjectUseCase = updateProjectUseCase ?? throw new ArgumentNullException(nameof(updateProjectUseCase));
            _deleteProjectUseCase = deleteProjectUseCase ?? throw new ArgumentNullException(nameof(deleteProjectUseCase));
            _configureWhatsappUseCase = configureWhatsappUseCase ?? throw new ArgumentNullException(nameof(configureWhatsappUseCase));
            _getPublicProjectsUseCase = getPublicProjectsUseCase ?? throw new ArgumentNullException(nameof(getPublicProjectsUseCase));
            _getPublicProjectEmployeesUseCase = getPublicProjectEmployeesUseCase ?? throw new ArgumentNullException(nameof(getPublicProjectEmployeesUseCase));
            _uploadProjectPhotoUseCase = uploadProjectPhotoUseCase ?? throw new ArgumentNullException(nameof(uploadProjectPhotoUseCase));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private bool ValidateApiKey()
        {
            try 
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                var apiKey = authHeader.Replace("Bearer ", "").Replace("Bearer", "").Trim();
                var expectedApiKey = _configuration["INTEGRATIONS_API_KEY"];
                return !string.IsNullOrEmpty(apiKey) && apiKey == expectedApiKey;
            }
            catch
            {
                return false;
            }
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
        /// Sube una foto para un proyecto (Máximo 10 fotos)
        /// </summary>
        [HttpPost("{projectId}/photos")]
        public async Task<IActionResult> UploadPhoto(Guid projectId, [FromForm] MeetLines.API.DTOs.UploadProjectPhotoRequest request, CancellationToken ct)
        {
            var userId = GetUserId();
            var file = request.File;

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded" });
            }

            // Validar tipo de archivo (solo imágenes)
            if (!file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new { error = "File must be an image" });
            }

            try 
            {
                using var stream = file.OpenReadStream();
                var result = await _uploadProjectPhotoUseCase.ExecuteAsync(userId, projectId, stream, file.FileName, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex) // Límite de 10 fotos alcanzado
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
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
        /// Obtiene todos los proyectos públicos (activos)
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProjects([FromQuery] double? latitude, [FromQuery] double? longitude, CancellationToken ct)
        {
            var result = await _getPublicProjectsUseCase.ExecuteAsync(latitude, longitude, ct);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });
            return Ok(result.Value);
        }

        /// <summary>
        /// Obtiene los empleados públicos (activos) de un proyecto
        /// </summary>
        [HttpGet("{projectId}/employees/public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProjectEmployees(Guid projectId, CancellationToken ct)
        {
            var result = await _getPublicProjectEmployeesUseCase.ExecuteAsync(projectId, ct);
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
            [FromServices] ILogger<ProjectsController> logger,
            CancellationToken ct = default)
        {
            try
            {
                logger.LogInformation("🔍 GetProjectPublic called for ProjectId: {ProjectId}", projectId);
                
                // Validar API Key
                if (!ValidateApiKey())
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
            // Validar API Key
            if (!ValidateApiKey())
            {
                return Unauthorized(new { error = "Invalid API Key" });
            }

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
