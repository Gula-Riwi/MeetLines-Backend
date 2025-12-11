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
        Task<Result<string>> CreateTransferAsync(CreateTransferRequest request, string userId, CancellationToken ct = default);
        Task<Result<LoginResponse>> AcceptTransferAsync(AcceptTransferRequest request, CancellationToken ct = default);
        Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
        Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);
        Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
        Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
        Task<Result> ResendVerificationEmailAsync(string email, CancellationToken ct = default);
        Task<Result> LogoutAsync(string refreshToken, CancellationToken ct = default);
        
        // Employee Password Management
        Task<Result> EmployeeForgotPasswordAsync(EmployeeForgotPasswordRequest request, CancellationToken ct = default);
        Task<Result> EmployeeResetPasswordAsync(EmployeeResetPasswordRequest request, CancellationToken ct = default);
        Task<Result> EmployeeChangePasswordAsync(Guid employeeId, EmployeeChangePasswordRequest request, CancellationToken ct = default);
        Task<Result<LoginResponse>> EmployeeLoginAsync(EmployeeLoginRequest request, CancellationToken ct = default);
    }
}