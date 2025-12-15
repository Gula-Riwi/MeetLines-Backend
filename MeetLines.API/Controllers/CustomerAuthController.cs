using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.Services.Interfaces;
using MeetLines.API.DTOs;
using MeetLines.Application.DTOs.Auth;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/customer-auth")]
    public class CustomerAuthController : ControllerBase
    {
        private readonly ICustomerAuthService _customerAuthService;

        public CustomerAuthController(ICustomerAuthService customerAuthService)
        {
            _customerAuthService = customerAuthService ?? throw new ArgumentNullException(nameof(customerAuthService));
        }

        /// <summary>
        /// Authenticate or Register an End-User (Customer) via Google OAuth
        /// POST: api/customer-auth/oauth/google
        /// Body: { code: string, redirectUri: string }
        /// </summary>
        [HttpPost("oauth/google")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GoogleOAuth([FromBody] DiscordExchangeRequest request, CancellationToken ct) 
        {
            // Note: Reusing DiscordExchangeRequest which has Code and RedirectUri properties
            try 
            {
               if (string.IsNullOrWhiteSpace(request.Code))
                    return BadRequest(ApiResponse.Fail("Code is required"));

               var ipAddress = GetClientIp() ?? "Unknown";
               var deviceInfo = GetUserAgent() ?? "Unknown";

               var result = await _customerAuthService.GoogleOAuthLoginAsync(request.Code, request.RedirectUri ?? "", ipAddress, deviceInfo, ct);

               if (!result.IsSuccess)
                   return BadRequest(ApiResponse<LoginResponse>.Fail(result.Error ?? "Google login failed"));

               return Ok(ApiResponse<LoginResponse>.Ok(result.Value!));
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
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        // Helper: read User-Agent header
        private string? GetUserAgent()
        {
            if (Request.Headers.TryGetValue("User-Agent", out var ua))
                return ua.ToString();
            return null;
        }
    }
}
