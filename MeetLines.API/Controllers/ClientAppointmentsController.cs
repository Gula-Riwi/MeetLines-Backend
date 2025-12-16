using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.UseCases.Client.Appointments;
using System.Security.Claims;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/client/appointments")]
    [Authorize] // Requires valid JWT
    public class ClientAppointmentsController : ControllerBase
    {
        private readonly IClientAppointmentUseCase _useCase;

        public ClientAppointmentsController(IClientAppointmentUseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAppointments([FromQuery] bool pendingOnly = false)
        {
            var userId = GetUserId();
            var result = await _useCase.GetMyAppointmentsAsync(userId, pendingOnly);
            
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(result.Error);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var userId = GetUserId();
            var result = await _useCase.CancelMyAppointmentAsync(userId, id);

            if (result.IsSuccess) return Ok(new { message = "Cita cancelada exitosamente." });
            return BadRequest(new { message = result.Error });
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // The token logic might vary (NameIdentifier vs "sub"). Assuming standard here given ClientAuthController. 
            // In ClientAuthController we see: 
            // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            // It parses correctly.
            if (Guid.TryParse(claim, out var id)) return id;
            throw new UnauthorizedAccessException("Usuario no identificado");
        }
    }
}
