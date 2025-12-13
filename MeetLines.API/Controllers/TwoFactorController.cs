using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.TwoFactor;
using MeetLines.Application.UseCases.TwoFactor;

namespace MeetLines.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar autenticación de dos factores (2FA) para SaasUser
    /// </summary>
    [ApiController]
    [Route("api/saas-auth/2fa")]
    [Authorize]
    public class TwoFactorController : ControllerBase
    {
        private readonly Enable2FAUseCase _enable2FAUseCase;
        private readonly Disable2FAUseCase _disable2FAUseCase;
        private readonly Verify2FACodeUseCase _verify2FAUseCase;

        public TwoFactorController(
            Enable2FAUseCase enable2FAUseCase,
            Disable2FAUseCase disable2FAUseCase,
            Verify2FACodeUseCase verify2FAUseCase)
        {
            _enable2FAUseCase = enable2FAUseCase ?? throw new ArgumentNullException(nameof(enable2FAUseCase));
            _disable2FAUseCase = disable2FAUseCase ?? throw new ArgumentNullException(nameof(disable2FAUseCase));
            _verify2FAUseCase = verify2FAUseCase ?? throw new ArgumentNullException(nameof(verify2FAUseCase));
        }

        /// <summary>
        /// Verifica el código TOTP durante la configuración
        /// </summary>
        /// <param name="request">Código TOTP de 6 dígitos</param>
        [HttpPost("verify")]
        [ProducesResponseType(typeof(TwoFactorResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FASetupRequest request, CancellationToken ct)
        {
            try
            {
                var userId = GetUserId();
                var response = await _verify2FAUseCase.ExecuteAsync(userId, request.Code, ct);
                
                if (!response.Success)
                    return BadRequest(response);
                
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Habilita 2FA para el usuario autenticado
        /// Genera secreto TOTP, QR code y códigos de respaldo
        /// </summary>
        /// <returns>Secreto, URI del QR y códigos de respaldo</returns>
        [HttpPost("enable")]
        [ProducesResponseType(typeof(Enable2FAResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Enable2FA(CancellationToken ct)
        {
            try
            {
                var userId = GetUserId();
                var response = await _enable2FAUseCase.ExecuteAsync(userId, ct);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deshabilita 2FA para el usuario autenticado
        /// Requiere código TOTP válido para confirmar
        /// </summary>
        /// <param name="request">Código TOTP de 6 dígitos</param>
        [HttpPost("disable")]
        [ProducesResponseType(typeof(TwoFactorResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Disable2FA([FromBody] Disable2FARequest request, CancellationToken ct)
        {
            try
            {
                var userId = GetUserId();
                var response = await _disable2FAUseCase.ExecuteAsync(userId, request.Code, ct);
                
                if (!response.Success)
                    return BadRequest(response);
                
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
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
}
