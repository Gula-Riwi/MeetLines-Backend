using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.Appointments;
using MeetLines.Application.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IConfiguration _configuration;

        public AppointmentsController(IAppointmentService appointmentService, IConfiguration configuration)
        {
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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
        /// Gets all active services for a project
        /// Used by n8n - requires API key authentication
        /// </summary>
        [HttpGet("services")]
        [AllowAnonymous]
        public async Task<ActionResult> GetServices(Guid projectId, CancellationToken ct = default)
        {
            // Validate API key for n8n integration
            if (!ValidateApiKey())
            {
                return Unauthorized(new { error = "Invalid or missing API key" });
            }

            var services = await _appointmentService.GetServicesAsync(projectId, ct);
            return Ok(services);
        }

        /// <summary>
        /// Gets all active services for a project (Public endpoint)
        /// Used by frontend widget - no authentication required
        /// </summary>
        [HttpGet("services/public")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPublicServices(Guid projectId, CancellationToken ct = default)
        {
            var services = await _appointmentService.GetServicesAsync(projectId, ct);
            
            var publicServices = services.Select(s => new MeetLines.Application.DTOs.Appointments.ServicePublicDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                Currency = s.Currency,
                DurationMinutes = s.DurationMinutes
            });

            return Ok(publicServices);
        }

        /// <summary>
        /// Gets available appointment slots for a specific date
        /// Used by n8n - requires API key authentication
        /// </summary>
        [HttpGet("appointments/available-slots")]
        [AllowAnonymous]
        public async Task<ActionResult> GetAvailableSlots(
            Guid projectId,
            [FromQuery] string date,
            [FromQuery] int? serviceId = null,
            CancellationToken ct = default)
        {
            try
            {
                // Validate API key for n8n integration
                if (!ValidateApiKey())
                {
                    return Unauthorized(new { error = "Invalid or missing API key" });
                }

                if (!DateTime.TryParse(date, out var parsedDate))
                {
                    return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd" });
                }

                var slots = await _appointmentService.GetAvailableSlotsAsync(projectId, parsedDate, serviceId, ct);
                return Ok(slots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Ocurrió un error interno en el servidor.", 
                    details = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Creates a new appointment
        /// Used by n8n - requires API key authentication
        /// </summary>
        [HttpPost("appointments")]
        [AllowAnonymous]
        public async Task<ActionResult> CreateAppointment(
            Guid projectId,
            [FromBody] CreateAppointmentRequest request,
            CancellationToken ct = default)
        {
            // Security Check: Allow if API Key is valid OR User is Authenticated (JWT)
            bool isApiKeyValid = ValidateApiKey();
            bool isUserAuthenticated = User.Identity?.IsAuthenticated == true;

            if (!isApiKeyValid && !isUserAuthenticated)
            {
                return Unauthorized(new { error = "Unauthorized: Missing valid API Key or JWT Token" });
            }

            // Optional: If user is authenticated, we could force UserId from token into request logic
            // But service handles lookups by Email nicely.

            if (request.ProjectId != projectId)
            {
                return BadRequest(new { message = "Project ID mismatch" });
            }

            var result = await _appointmentService.CreateAppointmentAsync(request, ct);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Gets appointments for a project
        /// Requires authentication
        /// </summary>
        [HttpGet("appointments")]
        [Authorize]
        public async Task<ActionResult> GetAppointments(
            Guid projectId,
            CancellationToken ct = default)
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
            var userRole = User.FindFirst("role")?.Value ?? "user";

            var result = await _appointmentService.GetAppointmentsAsync(userId, userRole, projectId, ct);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }
    }
}
