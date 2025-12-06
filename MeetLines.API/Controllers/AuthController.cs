using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.Auth;
using MeetLines.Application.Services.Interfaces;
using MeetLines.API.DTOs;
using FluentValidation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

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

        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// Exchange a Discord authorization code for user info and perform OAuth login server-side.
        /// POST: api/auth/oauth/discord
        /// Body: { code: string, redirectUri?: string }
        /// </summary>
        [HttpPost("oauth/discord")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DiscordOAuthExchange([FromBody] DiscordExchangeRequest request, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Code))
                    return BadRequest(ApiResponse.Fail("Code is required"));

                var clientId = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET");
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                    return StatusCode(500, ApiResponse.Fail("Discord OAuth is not configured on the server"));

                // Exchange code for access token
                using var http = new HttpClient();
                var form = new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["grant_type"] = "authorization_code",
                    ["code"] = request.Code,
                    ["redirect_uri"] = request.RedirectUri ?? ""
                };

                var tokenResp = await http.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(form), ct);
                if (!tokenResp.IsSuccessStatusCode)
                {
                    var err = await tokenResp.Content.ReadAsStringAsync(ct);
                    return BadRequest(ApiResponse.Fail($"Discord token exchange failed: {err}"));
                }

                var tokenJson = await tokenResp.Content.ReadAsStringAsync(ct);
                using var tokenDoc = JsonDocument.Parse(tokenJson);
                var root = tokenDoc.RootElement;
                if (!root.TryGetProperty("access_token", out var accessTokenProp))
                    return BadRequest(ApiResponse.Fail("Discord token response did not contain access_token"));

                var accessToken = accessTokenProp.GetString();

                // Get user info
                var req = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var userResp = await http.SendAsync(req, ct);
                if (!userResp.IsSuccessStatusCode)
                {
                    var err = await userResp.Content.ReadAsStringAsync(ct);
                    return BadRequest(ApiResponse.Fail($"Discord user info failed: {err}"));
                }

                var userJson = await userResp.Content.ReadAsStringAsync(ct);
                using var userDoc = JsonDocument.Parse(userJson);
                var u = userDoc.RootElement;

                var externalId = u.GetProperty("id").GetString() ?? string.Empty;
                var username = u.GetProperty("username").GetString() ?? string.Empty;
                string email = string.Empty;
                if (u.TryGetProperty("email", out var emailProp) && emailProp.ValueKind == JsonValueKind.String)
                    email = emailProp.GetString() ?? string.Empty;

                var oauthRequest = new OAuthLoginRequest
                {
                    ExternalProviderId = externalId,
                    Email = email,
                    Name = username,
                    Provider = MeetLines.Domain.Enums.AuthProvider.Discord,
                    DeviceInfo = GetUserAgent(),
                    IpAddress = GetClientIp()
                };

                var result = await _authService.OAuthLoginAsync(oauthRequest, ct);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<LoginResponse>.Fail(result.Error ?? "OAuth login failed"));

                return Ok(ApiResponse<LoginResponse>.Ok(result.Value ?? new LoginResponse()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Internal error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Exchange a Facebook authorization code for user info and perform OAuth login server-side.
        /// POST: api/auth/oauth/facebook
        /// Body: { code: string, redirectUri?: string }
        /// </summary>
        [HttpPost("oauth/facebook")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> FacebookOAuthExchange([FromBody] DiscordExchangeRequest request, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Code))
                    return BadRequest(ApiResponse.Fail("Code is required"));

                var appId = Environment.GetEnvironmentVariable("FB_APP_ID");
                var appSecret = Environment.GetEnvironmentVariable("FB_APP_SECRET");
                if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
                    return StatusCode(500, ApiResponse.Fail("Facebook OAuth is not configured on the server"));

                // Exchange code for access token
                using var http = new HttpClient();
                var tokenUrl = $"https://graph.facebook.com/v18.0/oauth/access_token?" +
                    $"client_id={appId}&" +
                    $"client_secret={appSecret}&" +
                    $"code={request.Code}&" +
                    $"redirect_uri={Uri.EscapeDataString(request.RedirectUri ?? "")}";

                var tokenResp = await http.GetAsync(tokenUrl, ct);
                if (!tokenResp.IsSuccessStatusCode)
                {
                    var err = await tokenResp.Content.ReadAsStringAsync(ct);
                    return BadRequest(ApiResponse.Fail($"Facebook token exchange failed: {err}"));
                }

                var tokenJson = await tokenResp.Content.ReadAsStringAsync(ct);
                using var tokenDoc = JsonDocument.Parse(tokenJson);
                var root = tokenDoc.RootElement;
                if (!root.TryGetProperty("access_token", out var accessTokenProp))
                    return BadRequest(ApiResponse.Fail("Facebook token response did not contain access_token"));

                var accessToken = accessTokenProp.GetString();

                // Get user info
                var userUrl = $"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={accessToken}";
                var userResp = await http.GetAsync(userUrl, ct);
                if (!userResp.IsSuccessStatusCode)
                {
                    var err = await userResp.Content.ReadAsStringAsync(ct);
                    return BadRequest(ApiResponse.Fail($"Facebook user info failed: {err}"));
                }

                var userJson = await userResp.Content.ReadAsStringAsync(ct);
                using var userDoc = JsonDocument.Parse(userJson);
                var u = userDoc.RootElement;

                var externalId = u.GetProperty("id").GetString() ?? string.Empty;
                var name = u.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "Facebook User";
                string? email = null;
                if (u.TryGetProperty("email", out var emailProp) && emailProp.ValueKind == JsonValueKind.String)
                    email = emailProp.GetString();

                var oauthRequest = new OAuthLoginRequest
                {
                    ExternalProviderId = externalId,
                    Email = email,
                    Name = name,
                    Provider = MeetLines.Domain.Enums.AuthProvider.Facebook,
                    DeviceInfo = GetUserAgent(),
                    IpAddress = GetClientIp()
                };

                var result = await _authService.OAuthLoginAsync(oauthRequest, ct);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<LoginResponse>.Fail(result.Error ?? "OAuth login failed"));

                return Ok(ApiResponse<LoginResponse>.Ok(result.Value ?? new LoginResponse()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Internal error: {ex.Message}"));
            }
        }
        /// POST: api/auth/create-transfer
        /// Body: { tenant: string }
        /// Requires Authorization: Bearer <accessToken>
        /// </summary>
        [HttpPost("create-transfer")]
        public async Task<IActionResult> CreateTransfer([FromBody] MeetLines.Application.DTOs.Auth.CreateTransferRequest request, CancellationToken ct)
        {
            try
            {
                // Ensure user is authenticated
                if (!User.Identity?.IsAuthenticated ?? true)
                    return Unauthorized(ApiResponse.Fail("Unauthorized"));

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse.Fail("User id not found in token"));

                var result = await _authService.CreateTransferAsync(request, userId, ct);
                if (!result.IsSuccess) return BadRequest(ApiResponse.Fail(result.Error ?? "Error creating transfer"));

                return Ok(ApiResponse<string>.Ok(result.Value ?? string.Empty));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Internal error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Accept a transfer token (used by tenant subdomain) and returns LoginResponse tokens.
        /// POST: api/auth/accept-transfer
        /// Body: { transferToken: string }
        /// </summary>
        [HttpPost("accept-transfer")]
        public async Task<IActionResult> AcceptTransfer([FromBody] MeetLines.Application.DTOs.Auth.AcceptTransferRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _authService.AcceptTransferAsync(request, ct);
                if (!result.IsSuccess) return BadRequest(ApiResponse.Fail(result.Error ?? "Error accepting transfer"));

                return Ok(ApiResponse<LoginResponse>.Ok(result.Value ?? new LoginResponse()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Internal error: {ex.Message}"));
            }
        }

        // Helper: try to read client IP respecting X-Forwarded-For
        private string? GetClientIp()
        {
            var headers = Request.Headers;
            if (headers.ContainsKey("X-Forwarded-For"))
            {
                var xff = headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrWhiteSpace(xff))
                {
                    var first = xff.Split(',')[0].Trim();
                    if (!string.IsNullOrEmpty(first)) return first;
                }
            }

            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            return remoteIp?.ToString();
        }

        // Helper: read User-Agent header
        private string? GetUserAgent()
        {
            if (Request.Headers.TryGetValue("User-Agent", out var ua))
                return ua.ToString();
            return null;
        }

        // Helper: try common timezone headers
        private string? GetTimezoneFromHeaders()
        {
            var headers = Request.Headers;
            string? tz = null;
            if (headers.ContainsKey("Timezone")) tz = headers["Timezone"].ToString();
            if (string.IsNullOrWhiteSpace(tz) && headers.ContainsKey("Time-Zone")) tz = headers["Time-Zone"].ToString();
            if (string.IsNullOrWhiteSpace(tz) && headers.ContainsKey("X-Timezone")) tz = headers["X-Timezone"].ToString();
            if (string.IsNullOrWhiteSpace(tz) && headers.ContainsKey("X-Time-Zone")) tz = headers["X-Time-Zone"].ToString();
            return string.IsNullOrWhiteSpace(tz) ? null : tz;
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
                request.Timezone = GetTimezoneFromHeaders() ?? request.Timezone;
                request.IpAddress = request.IpAddress ?? GetClientIp();
                request.DeviceInfo = request.DeviceInfo ?? GetUserAgent();

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
                request.IpAddress = request.IpAddress ?? GetClientIp();
                request.DeviceInfo = request.DeviceInfo ?? GetUserAgent();

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
        /// Inicia sesión para empleados de un proyecto.
        /// POST: api/auth/employee-login
        /// </summary>
        [HttpPost("employee-login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> EmployeeLogin([FromBody] EmployeeLoginRequest request, CancellationToken ct)
        {
            try
            {
                // Auto-populate ip/device when missing
                request.IpAddress = request.IpAddress ?? GetClientIp();
                request.DeviceInfo = request.DeviceInfo ?? GetUserAgent();

                var result = await _authService.EmployeeLoginAsync(request, ct);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<LoginResponse>.Fail(result.Error ?? "Error en login de empleado"));

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
                request.IpAddress = request.IpAddress ?? GetClientIp();
                request.DeviceInfo = request.DeviceInfo ?? GetUserAgent();

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
                request.IpAddress = request.IpAddress ?? GetClientIp();
                request.DeviceInfo = request.DeviceInfo ?? GetUserAgent();

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
                request.IpAddress = request.IpAddress ?? GetClientIp();

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
