using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.Auth;
using MeetLines.Application.Services.Interfaces;
using MeetLines.API.DTOs;
using FluentValidation;

namespace MeetLines.API.Controllers
{
    /// <summary>
    /// Adaptador de entrada (Puerto) para autenticación.
    /// Implementa DDD hexagonal: convierte las solicitudes HTTP en casos de uso de aplicación.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IHttpContextInfoService _contextInfoService;

        public AuthController(IAuthService authService, IHttpContextInfoService contextInfoService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _contextInfoService = contextInfoService ?? throw new ArgumentNullException(nameof(contextInfoService));
        }

        /// <summary>
        /// Registra un nuevo usuario con email y contraseña.
        /// POST: api/auth/register
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
        {
            try
            {
                // Auto-populate timezone, ip and device info when possible
                var contextInfo = _contextInfoService.GetContextInfo();
                request.Timezone = contextInfo.Timezone ?? request.Timezone;
                request.IpAddress = request.IpAddress ?? contextInfo.IpAddress;
                request.DeviceInfo = request.DeviceInfo ?? contextInfo.DeviceInfo;

                // FluentValidation se ejecutará automáticamente por el middleware
                // Ejecutar caso de uso
                var result = await _authService.RegisterAsync(request, ct);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<RegisterResponse>.Fail(result.Error ?? "Error en registro"));

                return Ok(ApiResponse<RegisterResponse>.Ok(
                    result.Value!,
                    "Registro exitoso. Por favor verifica tu email."
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Inicia sesión con email y contraseña.
        /// POST: api/auth/login
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            try
            {
                // Auto-populate ip/device when missing
                var contextInfo = _contextInfoService.GetContextInfo();
                request.IpAddress = request.IpAddress ?? contextInfo.IpAddress;
                request.DeviceInfo = request.DeviceInfo ?? contextInfo.DeviceInfo;

                // Ejecutar caso de uso
                var result = await _authService.LoginAsync(request, ct);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<LoginResponse>.Fail(result.Error ?? "Error en login"));

                return Ok(ApiResponse<LoginResponse>.Ok(result.Value ?? new LoginResponse()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Inicia sesión con OAuth (Google, Facebook, etc).
        /// POST: api/auth/oauth-login
        /// </summary>
        [HttpPost("oauth-login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request, CancellationToken ct)
        {
            try
            {
                // Auto-populate ip/device when missing
                var contextInfo = _contextInfoService.GetContextInfo();
                request.IpAddress = request.IpAddress ?? contextInfo.IpAddress;
                request.DeviceInfo = request.DeviceInfo ?? contextInfo.DeviceInfo;

                // Ejecutar caso de uso
                var result = await _authService.OAuthLoginAsync(request, ct);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<LoginResponse>.Fail(result.Error ?? "Error en OAuth login"));

                return Ok(ApiResponse<LoginResponse>.Ok(result.Value ?? new LoginResponse()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Refresca el token de acceso usando un refresh token válido.
        /// POST: api/auth/refresh-token
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            try
            {
                // Auto-populate ip/device when missing
                var contextInfo = _contextInfoService.GetContextInfo();
                request.IpAddress = request.IpAddress ?? contextInfo.IpAddress;
                request.DeviceInfo = request.DeviceInfo ?? contextInfo.DeviceInfo;

                var result = await _authService.RefreshTokenAsync(request, ct);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<LoginResponse>.Fail(result.Error ?? "Error al refrescar token"));

                return Ok(ApiResponse<LoginResponse>.Ok(result.Value ?? new LoginResponse()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Verifica el email del usuario usando un token de verificación.
        /// POST: api/auth/verify-email
        /// </summary>
        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _authService.VerifyEmailAsync(request, ct);
                
                if (!result.IsSuccess)
                    return BadRequest(ApiResponse.Fail(result.Error ?? "Error al verificar email"));
                
                return Ok(ApiResponse.Ok("Email verificado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Solicita un email de recuperación de contraseña.
        /// POST: api/auth/forgot-password
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
        {
            try
            {
                // Auto-populate ip when missing
                request.IpAddress = request.IpAddress ?? _contextInfoService.GetClientIpAddress();

                var result = await _authService.ForgotPasswordAsync(request, ct);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponse.Fail(result.Error ?? "Error al solicitar recuperación"));

                return Ok(ApiResponse.Ok("Se ha enviado un email de recuperación a tu correo"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Restablece la contraseña usando un token válido.
        /// POST: api/auth/reset-password
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(request, ct);
                
                if (!result.IsSuccess)
                    return BadRequest(ApiResponse.Fail(result.Error ?? "Error al restablecer contraseña"));
                
                return Ok(ApiResponse.Ok("Contraseña restablecida exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reenvía el email de verificación de email.
        /// POST: api/auth/resend-verification-email
        /// </summary>
        [HttpPost("resend-verification-email")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ForgotPasswordRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _authService.ResendVerificationEmailAsync(request.Email, ct);
                
                if (!result.IsSuccess)
                    return BadRequest(ApiResponse.Fail(result.Error ?? "Error al reenviar verificación"));
                
                return Ok(ApiResponse.Ok("Email de verificación reenviado"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cierra sesión invalidando el refresh token.
        /// POST: api/auth/logout
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
