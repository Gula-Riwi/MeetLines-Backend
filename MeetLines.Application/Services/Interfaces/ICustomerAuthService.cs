using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Auth;
using MeetLines.Domain.Enums;

namespace MeetLines.Application.Services.Interfaces
{
    public interface ICustomerAuthService
    {
        /// <summary>
        /// Authenticates an AppUser (End Customer) using Google OAuth Code
        /// </summary>
        Task<Result<LoginResponse>> GoogleOAuthLoginAsync(string code, string redirectUri, string ipAddress, string deviceInfo, CancellationToken ct = default);
    }
}
