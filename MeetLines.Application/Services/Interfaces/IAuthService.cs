using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Auth;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<Result<LoginResponse>> OAuthLoginAsync(OAuthLoginRequest request, CancellationToken ct = default);
        Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
        Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);
        Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
        Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
        Task<Result> ResendVerificationEmailAsync(string email, CancellationToken ct = default);
        Task<Result> LogoutAsync(string refreshToken, CancellationToken ct = default);
    }
}