using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.Appointments;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IProjectRepository _projectRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IProjectRepository projectRepository,
            IEmployeeRepository employeeRepository)
        {
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request, CancellationToken ct)
        {
            var result = await _appointmentService.CreateAppointmentAsync(request, ct);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAppointments([FromQuery] Guid? projectId, CancellationToken ct)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            // Determine Role. 
            // In typical JWT, role claim might be "role" or ClaimTypes.Role.
            // AuthService uses ClaimTypes.Role? Let's check. 
            // AuthService: GenerateAccessToken(user.Id, user.Email, "User");
            // EmployeeService: GenerateAccessToken(employee.Id, employee.Username, employee.Role); 
            // But JwtTokenService might use a standard claim name. 
            // Let's look for "role" claim or ClaimTypes.Role.
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

            Guid targetProjectId = Guid.Empty;

            if (role == "Employee") // Assuming "Employee" is the role string, or "Barber", etc.
            {
                // Retrieve Employee to get ProjectId
                var employee = await _employeeRepository.GetByIdAsync(userId, ct);
                if (employee == null) return Unauthorized("Employee profile not found");
                targetProjectId = employee.ProjectId;
            }
            else // "User" or Owner
            {
                // Validate if user owns the project requested, or default to first project
                if (projectId.HasValue)
                {
                    var isOwner = await _projectRepository.IsUserProjectOwnerAsync(userId, projectId.Value, ct);
                    if (!isOwner) return Forbid("You do not own this project");
                    targetProjectId = projectId.Value;
                }
                else
                {
                    // Default to first active project
                    var projects = await _projectRepository.GetByUserAsync(userId, ct);
                    var project = projects.FirstOrDefault();
                    if (project == null) return BadRequest("User has no projects");
                    targetProjectId = project.Id;
                }
            }

            var result = await _appointmentService.GetAppointmentsAsync(userId, role ?? "User", targetProjectId, ct);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
    }
}
