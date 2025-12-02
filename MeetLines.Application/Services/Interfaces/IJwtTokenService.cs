using System;
using System.Security.Claims;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(Guid userId, string email, string role);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
        string ComputeTokenHash(string token);
    }
}