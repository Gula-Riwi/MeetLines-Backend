using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Application.UseCases.Projects;
using MeetLines.Domain.Repositories;
using MeetLines.Application.UseCases.Projects.Interfaces;
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
        private readonly IGetPublicProjectsUseCase _getPublicProjectsUseCase;
        private readonly IGetPublicProjectEmployeesUseCase _getPublicProjectEmployeesUseCase;
        private readonly IUploadProjectPhotoUseCase _uploadProjectPhotoUseCase;
        private readonly IUploadProjectProfilePhotoUseCase _uploadProjectProfilePhotoUseCase;
        private readonly IGetProjectPhotosUseCase _getProjectPhotosUseCase;
        private readonly IDeleteProjectPhotoUseCase _deleteProjectPhotoUseCase;
        private readonly IConfiguration _configuration;

        public ProjectsController(
            ICreateProjectUseCase createProjectUseCase,
            IGetUserProjectsUseCase getUserProjectsUseCase,
            IGetProjectByIdUseCase getProjectByIdUseCase,
            IUpdateProjectUseCase updateProjectUseCase,
            IDeleteProjectUseCase deleteProjectUseCase,
            IConfigureWhatsappUseCase configureWhatsappUseCase,
            IConfigureTelegramUseCase configureTelegramUseCase,
            IGetPublicProjectsUseCase getPublicProjectsUseCase,
            IGetPublicProjectEmployeesUseCase getPublicProjectEmployeesUseCase,
            IUploadProjectPhotoUseCase uploadProjectPhotoUseCase,
            IUploadProjectProfilePhotoUseCase uploadProjectProfilePhotoUseCase,
            IGetProjectPhotosUseCase getProjectPhotosUseCase,
            IDeleteProjectPhotoUseCase deleteProjectPhotoUseCase,
            IConfiguration configuration)
        {
            _createProjectUseCase = createProjectUseCase ?? throw new ArgumentNullException(nameof(createProjectUseCase));
            _getUserProjectsUseCase = getUserProjectsUseCase ?? throw new ArgumentNullException(nameof(getUserProjectsUseCase));
            _getProjectByIdUseCase = getProjectByIdUseCase ?? throw new ArgumentNullException(nameof(getProjectByIdUseCase));
            _updateProjectUseCase = updateProjectUseCase ?? throw new ArgumentNullException(nameof(updateProjectUseCase));
            _deleteProjectUseCase = deleteProjectUseCase ?? throw new ArgumentNullException(nameof(deleteProjectUseCase));
            _configureWhatsappUseCase = configureWhatsappUseCase ?? throw new ArgumentNullException(nameof(configureWhatsappUseCase));
            _configureTelegramUseCase = configureTelegramUseCase ?? throw new ArgumentNullException(nameof(configureTelegramUseCase));
            _getPublicProjectsUseCase = getPublicProjectsUseCase ?? throw new ArgumentNullException(nameof(getPublicProjectsUseCase));
            _getPublicProjectEmployeesUseCase = getPublicProjectEmployeesUseCase ?? throw new ArgumentNullException(nameof(getPublicProjectEmployeesUseCase));
            _uploadProjectPhotoUseCase = uploadProjectPhotoUseCase ?? throw new ArgumentNullException(nameof(uploadProjectPhotoUseCase));
            _uploadProjectProfilePhotoUseCase = uploadProjectProfilePhotoUseCase ?? throw new ArgumentNullException(nameof(uploadProjectProfilePhotoUseCase));
            _getProjectPhotosUseCase = getProjectPhotosUseCase ?? throw new ArgumentNullException(nameof(getProjectPhotosUseCase));
            _deleteProjectPhotoUseCase = deleteProjectPhotoUseCase ?? throw new ArgumentNullException(nameof(deleteProjectPhotoUseCase));
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
        /// Crea un nuevo proyecto/empresa con foto de perfil opcional
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromForm] MeetLines.API.DTOs.CreateProjectWithPhotoRequest formRequest, CancellationToken ct)
        {
            var userId = GetUserId();
            
            // Mapear el request del formulario al DTO de la aplicación
            var request = new CreateProjectRequest
            {
                Name = formRequest.Name,
                Industry = formRequest.Industry,
                Description = formRequest.Description,
                Address = formRequest.Address,
                City = formRequest.City,
                Country = formRequest.Country,
                Latitude = formRequest.Latitude,
                Longitude = formRequest.Longitude
            };

            // Validar archivo si se proporciona
            if (formRequest.ProfilePhoto != null)
            {
                if (formRequest.ProfilePhoto.Length == 0)
                {
                    return BadRequest(new { error = "Profile photo file is empty" });
                }

                if (!formRequest.ProfilePhoto.ContentType.StartsWith("image/"))
                {
                    return BadRequest(new { error = "Profile photo must be an image" });
                }
            }

            // Ejecutar el use case con el archivo opcional
            Result<ProjectResponse> result;
            if (formRequest.ProfilePhoto != null)
            {
                using var stream = formRequest.ProfilePhoto.OpenReadStream();
                result = await _createProjectUseCase.ExecuteAsync(userId, request, stream, formRequest.ProfilePhoto.FileName, ct);
            }
            else
            {
                result = await _createProjectUseCase.ExecuteAsync(userId, request, null, null, ct);
            }

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetProjectById), new { projectId = result.Value!.Id }, result.Value);
        }

        /// <summary>
        /// Actualiza un proyecto existente con foto de perfil opcional
        /// </summary>
        [HttpPut("{projectId}")]
        public async Task<IActionResult> UpdateProject(Guid projectId, [FromForm] MeetLines.API.DTOs.UpdateProjectWithPhotoRequest formRequest, CancellationToken ct)
        {
            var userId = GetUserId();
            
            // Mapear el request del formulario al DTO de la aplicación
            var request = new UpdateProjectRequest
            {
                Name = formRequest.Name,
                Subdomain = formRequest.Subdomain,
                Industry = formRequest.Industry,
                Description = formRequest.Description,
                Address = formRequest.Address,
                City = formRequest.City,
                Country = formRequest.Country,
                Latitude = formRequest.Latitude,
                Longitude = formRequest.Longitude,
                WhatsappVerifyToken = formRequest.WhatsappVerifyToken,
                WhatsappPhoneNumberId = formRequest.WhatsappPhoneNumberId,
                WhatsappAccessToken = formRequest.WhatsappAccessToken,
                WhatsappForwardWebhook = formRequest.WhatsappForwardWebhook
            };

            // Validar archivo si se proporciona
            if (formRequest.ProfilePhoto != null)
            {
                if (formRequest.ProfilePhoto.Length == 0)
                {
                    return BadRequest(new { error = "Profile photo file is empty" });
                }

                if (!formRequest.ProfilePhoto.ContentType.StartsWith("image/"))
                {
                    return BadRequest(new { error = "Profile photo must be an image" });
                }
            }

            // Ejecutar el use case con el archivo opcional
            Result<ProjectResponse> result;
            if (formRequest.ProfilePhoto != null)
            {
                using var stream = formRequest.ProfilePhoto.OpenReadStream();
                result = await _updateProjectUseCase.ExecuteAsync(userId, projectId, request, stream, formRequest.ProfilePhoto.FileName, ct);
            }
            else
            {
                result = await _updateProjectUseCase.ExecuteAsync(userId, projectId, request, null, null, ct);
            }

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
        /// Sube una foto de perfil para un proyecto
        /// </summary>
        [HttpPost("{projectId}/profile-photo")]
        public async Task<IActionResult> UploadProfilePhoto(Guid projectId, [FromForm] MeetLines.API.DTOs.UploadProjectPhotoRequest request, CancellationToken ct)
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
                var resultUrl = await _uploadProjectProfilePhotoUseCase.ExecuteAsync(userId, projectId, stream, file.FileName, ct);
                return Ok(new { url = resultUrl });
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
        /// Obtiene todas las fotos de un proyecto
        /// </summary>
        [HttpGet("{projectId}/photos")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProjectPhotos(Guid projectId, CancellationToken ct)
        {
            var result = await _getProjectPhotosUseCase.ExecuteAsync(projectId, ct);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
            return Ok(result.Value);
        }

        /// <summary>
        /// Elimina una foto de un proyecto
        /// </summary>
        [HttpDelete("{projectId}/photos/{photoId}")]
        public async Task<IActionResult> DeleteProjectPhoto(Guid projectId, Guid photoId, CancellationToken ct)
        {
            var result = await _deleteProjectPhotoUseCase.ExecuteAsync(projectId, photoId, ct);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
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
                    Description = project.Description ?? string.Empty,
                    Address = project.Address,
                    City = project.City,
                    Country = project.Country,
                    ProfilePhotoUrl = project.ProfilePhotoUrl
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
        /// Obtiene detalles públicos extendidos de un proyecto (sin autenticación)
        /// Incluye ubicación e industria. Usado por clientes públicos.
        /// GET: api/projects/{projectId}/details/public
        /// </summary>
        [HttpGet("{projectId}/details/public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProjectPublicDetailsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProjectPublicDetails(
            [FromRoute] Guid projectId,
            [FromServices] IProjectRepository projectRepository,
            CancellationToken ct = default)
        {
             var project = await projectRepository.GetAsync(projectId, ct);
             
             if (project == null || project.Status != "active")
             {
                 return NotFound(new { error = "Project not found or not active" });
             }

             var dto = new ProjectPublicDetailsDto
             {
                 Id = project.Id.ToString(),
                 Name = project.Name,
                 Industry = project.Industry ?? string.Empty,
                 Description = project.Description ?? string.Empty,
                 Address = project.Address ?? string.Empty,
                 City = project.City ?? string.Empty,
                 Country = project.Country ?? string.Empty,
                 Latitude = project.Latitude,
                 Longitude = project.Longitude,
                 ProfilePhotoUrl = project.ProfilePhotoUrl
             };

             return Ok(dto);
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
