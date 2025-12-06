using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Domain.Repositories;

namespace MeetLines.API.Controllers
{
    /// <summary>
    /// Controlador para exponer credenciales de proyectos (para n8n y integraciones externas).
    /// Requiere autenticación con API key.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectCredentialsController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly Microsoft.Extensions.Logging.ILogger<ProjectCredentialsController> _logger;
        private readonly string _apiKey;

        public ProjectCredentialsController(
            IProjectRepository projectRepository,
            Microsoft.Extensions.Logging.ILogger<ProjectCredentialsController> logger)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiKey = Environment.GetEnvironmentVariable("INTEGRATIONS_API_KEY") ?? string.Empty;
        }

        /// <summary>
        /// Obtiene las credenciales de WhatsApp de un proyecto.
        /// Requiere header: Authorization: Bearer {INTEGRATIONS_API_KEY}
        /// GET: /api/project-credentials/{projectId}/whatsapp
        /// </summary>
        [HttpGet("{projectId}/whatsapp")]
        [ProducesResponseType(typeof(WhatsappCredentialsResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetWhatsappCredentials(Guid projectId, CancellationToken ct)
        {
            try
            {
                // Validate API key
                if (!ValidateApiKey())
                    return Unauthorized(new ErrorResponse { Error = "Invalid or missing API key" });

                if (projectId == Guid.Empty)
                    return BadRequest(new ErrorResponse { Error = "Invalid project ID" });

                var project = await _projectRepository.GetAsync(projectId, ct);
                if (project == null)
                    return NotFound(new ErrorResponse { Error = "Project not found" });

                // Only return credentials if WhatsApp integration is configured
                if (string.IsNullOrWhiteSpace(project.WhatsappPhoneNumberId) || string.IsNullOrWhiteSpace(project.WhatsappAccessToken))
                    return NotFound(new ErrorResponse { Error = "WhatsApp integration not configured for this project" });

                var response = new WhatsappCredentialsResponse
                {
                    ProjectId = project.Id,
                    PhoneNumberId = project.WhatsappPhoneNumberId,
                    AccessToken = project.WhatsappAccessToken,
                    Subdomain = project.Subdomain
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving WhatsApp credentials for project {ProjectId}", projectId);
                return StatusCode(500, new ErrorResponse { Error = "Internal server error" });
            }
        }

        private bool ValidateApiKey()
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return false;

            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader))
                return false;

            var parts = authHeader.Split(' ');
            if (parts.Length != 2 || !string.Equals(parts[0], "Bearer", StringComparison.OrdinalIgnoreCase))
                return false;

            return string.Equals(parts[1], _apiKey, StringComparison.Ordinal);
        }
    }

    public class WhatsappCredentialsResponse
    {
        public Guid ProjectId { get; set; }
        public string PhoneNumberId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}
