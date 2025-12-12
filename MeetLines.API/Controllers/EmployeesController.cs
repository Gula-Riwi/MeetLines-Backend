using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.API.DTOs;
using MeetLines.Application.DTOs.Employees;
using MeetLines.Application.Services.Interfaces;
using System.Security.Claims;
using MeetLines.Application.DTOs.Auth;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IAuthService _authService;

        public EmployeesController(IEmployeeService employeeService, IAuthService authService)
        {
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// Crea un nuevo empleado para un proyecto.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request, CancellationToken ct)
        {
            try
            {
                // TODO: Verificar que el usuario autenticado sea el dueño del ProjectId enviado
                
                var result = await _employeeService.CreateEmployeeAsync(request, ct);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<EmployeeResponse>.Fail(result.Error ?? "Error al crear empleado"));

                return Ok(ApiResponse<EmployeeResponse>.Ok(result.Value!));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lista los empleados de un proyecto.
        /// GET: api/employees?projectId=...
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeResponse>>), 200)]
        public async Task<IActionResult> GetEmployees([FromQuery] Guid projectId, CancellationToken ct)
        {
            try
            {
                // TODO: Verificar ownership
                
                var result = await _employeeService.GetEmployeesByProjectAsync(projectId, ct);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponse.Fail(result.Error ?? "Error al obtener empleados"));

                return Ok(ApiResponse<IEnumerable<EmployeeResponse>>.Ok(result.Value!));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cierra sesión de empleado (invalida refresh token).
        /// POST: api/employees/logout
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _authService.LogoutAsync(request.RefreshToken, ct);
                
                if (!result.IsSuccess)
                    return BadRequest(ApiResponse.Fail(result.Error ?? "Error al cerrar sesión"));
                
                return Ok(ApiResponse.Ok("Sesión cerrada exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error interno: {ex.Message}"));
            }
        }
    }
}
