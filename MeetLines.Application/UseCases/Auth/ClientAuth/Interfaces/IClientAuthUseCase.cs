using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Auth;

namespace MeetLines.Application.UseCases.Auth.ClientAuth.Interfaces
{
    public interface IClientAuthUseCase
    {
        Task<Result<ClientAuthResponse>> LoginAsync(ClientLoginRequest request, CancellationToken ct = default);
        Task<Result<ClientAuthResponse>> RegisterAsync(ClientRegisterRequest request, CancellationToken ct = default);
        Task<Result> ForgotPasswordAsync(ClientForgotPasswordRequest request, CancellationToken ct = default);
        Task<Result> ResetPasswordAsync(ClientResetPasswordRequest request, CancellationToken ct = default);
    }
}
