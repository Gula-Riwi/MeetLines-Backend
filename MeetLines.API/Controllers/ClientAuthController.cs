using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.Auth;
using MeetLines.Application.UseCases.Auth.ClientAuth.Interfaces;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/client/auth")]
    public class ClientAuthController : ControllerBase
    {
        private readonly IClientAuthUseCase _clientAuthUseCase;

        public ClientAuthController(IClientAuthUseCase clientAuthUseCase)
        {
            _clientAuthUseCase = clientAuthUseCase;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] ClientLoginRequest request, CancellationToken ct)
        {
            var result = await _clientAuthUseCase.LoginAsync(request, ct);
            if (!result.IsSuccess)
            {
                return Unauthorized(new { error = result.Error }); // 401
            }
            return Ok(result.Value);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] ClientRegisterRequest request, CancellationToken ct)
        {
            var result = await _clientAuthUseCase.RegisterAsync(request, ct);
            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error }); // 400
            }
            return Ok(result.Value);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ClientForgotPasswordRequest request, CancellationToken ct)
        {
            var result = await _clientAuthUseCase.ForgotPasswordAsync(request, ct);
             // Always return OK for security (unless unexpected error)
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
            return Ok(new { message = "Si el email existe, se ha enviado un enlace de recuperación." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ClientResetPasswordRequest request, CancellationToken ct)
        {
            var result = await _clientAuthUseCase.ResetPasswordAsync(request, ct);
            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error });
            }
            return Ok(new { message = "Contraseña actualizada exitosamente." });
        }
    }
}
