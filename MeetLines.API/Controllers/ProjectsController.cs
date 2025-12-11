using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public ProjectsController(
            ICreateProjectUseCase createProjectUseCase,
            IGetUserProjectsUseCase getUserProjectsUseCase,
            IGetProjectByIdUseCase getProjectByIdUseCase,
            IUpdateProjectUseCase updateProjectUseCase,
            IDeleteProjectUseCase deleteProjectUseCase)
        {
            _createProjectUseCase = createProjectUseCase ?? throw new ArgumentNullException(nameof(createProjectUseCase));
            _getUserProjectsUseCase = getUserProjectsUseCase ?? throw new ArgumentNullException(nameof(getUserProjectsUseCase));
            _getProjectByIdUseCase = getProjectByIdUseCase ?? throw new ArgumentNullException(nameof(getProjectByIdUseCase));
            _updateProjectUseCase = updateProjectUseCase ?? throw new ArgumentNullException(nameof(updateProjectUseCase));
            _deleteProjectUseCase = deleteProjectUseCase ?? throw new ArgumentNullException(nameof(deleteProjectUseCase));
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
        /// POST /api/projects/{projectId}/telegram/configure
        /// 
        /// FLUJO:
        /// 1. Valida que el usuario sea dueño del proyecto
        /// 2. Actualiza el proyecto con el bot_token, username y forward_webhook
        /// 3. Configura el webhook en Telegram API apuntando a nuestro backend
        /// 4. Telegram enviará mensajes a: https://services.meet-lines.com/webhook/telegram/{botToken}
        /// </summary>
        [HttpPost("{projectId}/telegram/configure")]
        public async Task<IActionResult> ConfigureTelegram(
            Guid projectId,
            [FromBody] ConfigureTelegramRequest request,
            [FromServices] IProjectRepository projectRepository,
            [FromServices] IHttpClientFactory httpClientFactory,
            [FromServices] IConfiguration configuration,
            CancellationToken ct)
        {
            try
            {
                var userId = GetUserId();

                // 1️⃣ Verificar que el usuario sea dueño del proyecto
                var isOwner = await projectRepository.IsUserProjectOwnerAsync(userId, projectId, ct);
                if (!isOwner)
                {
                    return Forbid();
                }

                var project = await projectRepository.GetAsync(projectId, ct);
                if (project == null)
                {
                    return NotFound(new { error = "Project not found" });
                }

                // 2️⃣ Construir URL del webhook
                // Si viene customWebhookUrl, usarla (útil para testing en local con VPS)
                // Si no, construir automáticamente
                string webhookUrl;
                if (!string.IsNullOrWhiteSpace(request.CustomWebhookUrl))
                {
                    webhookUrl = request.CustomWebhookUrl;
                }
                else
                {
                    // Construir automáticamente (siempre HTTPS para Telegram)
                    var baseDomain = configuration["Multitenancy:BaseDomain"] ?? "meet-lines.com";
                    webhookUrl = $"https://{baseDomain}/webhook/telegram/{request.BotToken}";
                }

                // URL de n8n (si no viene en el request, usar por defecto)
                var forwardWebhook = request.ForwardWebhook;
                if (string.IsNullOrWhiteSpace(forwardWebhook))
                {
                    // Por defecto: mismo dominio pero puerto 5678 (n8n típico)
                    // En VPS ajustarás esto con variable de entorno
                    forwardWebhook = configuration["Webhooks:N8nBaseUrl"] ?? $"http://localhost:5678/webhook/telegram-bot";
                }

                // 3️⃣ Configurar webhook en Telegram
                var client = httpClientFactory.CreateClient();
                var telegramApiUrl = $"https://api.telegram.org/bot{request.BotToken}/setWebhook";
                
                var telegramRequest = new
                {
                    url = webhookUrl
                };

                var response = await client.PostAsJsonAsync(telegramApiUrl, telegramRequest, ct);
                var responseBody = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new
                    {
                        error = "Failed to configure Telegram webhook",
                        details = responseBody,
                        message = "Verifica que el bot token sea válido"
                    });
                }

                // 4️⃣ Actualizar proyecto con los datos de Telegram
                project.UpdateTelegramIntegration(
                    request.BotToken,
                    request.BotUsername,
                    forwardWebhook
                );

                await projectRepository.UpdateAsync(project, ct);

                return Ok(new
                {
                    message = "Telegram integration configured successfully",
                    webhook_url = webhookUrl,
                    forward_to = forwardWebhook,
                    bot_username = request.BotUsername,
                    telegram_response = responseBody
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = ex.Message
                });
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
