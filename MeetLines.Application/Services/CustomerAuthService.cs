using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Auth;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace MeetLines.Application.Services
{
    public class CustomerAuthService : ICustomerAuthService
    {
        private readonly IAppUserRepository _appUserRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILoginSessionRepository _loginSessionRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public CustomerAuthService(
            IAppUserRepository appUserRepository,
            IJwtTokenService jwtTokenService,
            ILoginSessionRepository loginSessionRepository,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _appUserRepository = appUserRepository;
            _jwtTokenService = jwtTokenService;
            _loginSessionRepository = loginSessionRepository;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<LoginResponse>> GoogleOAuthLoginAsync(string code, string redirectUri, string ipAddress, string deviceInfo, CancellationToken ct = default)
        {
            try
            {
                // 1. Get Credentials from Env
                var clientId = _configuration["Authentication:Google:ClientId"];
                var clientSecret = _configuration["Authentication:Google:ClientSecret"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return Result<LoginResponse>.Fail("Google OAuth is not configured on the server");
                }

                // 2. Exchange Code for Token
                var tokenUrl = "https://oauth2.googleapis.com/token";
                var tokenValues = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "code", code },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", redirectUri }
                };

                var client = _httpClientFactory.CreateClient();
                var tokenResponse = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(tokenValues), ct);

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    var error = await tokenResponse.Content.ReadAsStringAsync(ct);
                    return Result<LoginResponse>.Fail($"Google token exchange failed: {error}");
                }

                var tokenJson = await tokenResponse.Content.ReadAsStringAsync(ct);
                using var tokenDoc = JsonDocument.Parse(tokenJson);
                var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();
                // id_token is also available if needed for OpenID connect validation

                // 3. Get User Info
                var userInfoResponse = await client.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}", ct);
                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    return Result<LoginResponse>.Fail("Failed to get Google user info");
                }

                var userJson = await userInfoResponse.Content.ReadAsStringAsync(ct);
                using var userDoc = JsonDocument.Parse(userJson);
                var root = userDoc.RootElement;

                var googleId = root.GetProperty("id").GetString();
                var email = root.GetProperty("email").GetString();
                var name = root.GetProperty("name").GetString();
                var picture = root.TryGetProperty("picture", out var pic) ? pic.GetString() : null;

                if (string.IsNullOrEmpty(email)) return Result<LoginResponse>.Fail("Google account does not have an email");

                // 4. Find or Create User
                var user = await _appUserRepository.GetByExternalProviderIdAsync(googleId, ct);
                
                if (user == null)
                {
                    user = await _appUserRepository.GetByEmailAsync(email, ct);
                    if (user != null) 
                    {
                        // Link existing user? For now, we assume AppUser is pure customer.
                        // Ideally we warn or auto-link. Let's auto-link logic if supported by AppUser (it's immutable in constructor).
                        // AppUser doesn't have "LinkProvider" method publicly exposed in snippets I saw, but it has public props set in constructor.
                        // Actually AppUser properties are private set.
                        // We might need to extend AppUser to support linking or just accept email match.
                        // Existing AppUser was created by Bot? Then AuthProvider="bot".
                        // If we found by email, we treat it as the same user.
                    }
                }

                if (user == null)
                {
                    // Create new User
                    // AppUser(string email, string fullName, string? phone = null, string authProvider = "bot")
                    // We need a way to set ExternalProviderId. The constructor viewed earlier support authProvider but not externalId explicitly?
                    // Re-checking AppUser.cs...
                    // Constructor: public AppUser(string email, string fullName, string? phone = null, string authProvider = "bot")
                    // It does NOT accept externalProviderId.
                    // We need to FIX AppUser to support ExternalProviderId setting or Constructor.
                    
                    // user = new AppUser(email, name, null, "google");
                    // user.SetExternalId(googleId);
                    
                    user = AppUser.CreateOAuthUser(email, name, "google", googleId);
                    
                    await _appUserRepository.AddAsync(user, ct);
                }

                // 5. Generate Login Tokens (JWT)
                // Use "Customer" role to distinguish from "User" (SaaS Owner)?
                // Or just "AppUser"?
                var jwt = _jwtTokenService.GenerateAccessToken(user.Id, user.Email!, "AppUser");
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var tokenHash = _jwtTokenService.ComputeTokenHash(refreshToken);

                var session = new LoginSession(
                    user.Id,
                    tokenHash,
                    deviceInfo,
                    ipAddress,
                    DateTimeOffset.UtcNow.AddDays(7)
                );
                
                await _loginSessionRepository.AddAsync(session, ct);

                return Result<LoginResponse>.Ok(new LoginResponse
                {
                    AccessToken = jwt,
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                    Email = user.Email,
                    Name = user.FullName,
                    IsEmailVerified = user.IsEmailVerified,
                    ExpiresAt = session.ExpiresAt!.Value
                });

            }
            catch (Exception ex)
            {
                return Result<LoginResponse>.Fail($"Internal error: {ex.Message}");
            }
        }
    }
}
