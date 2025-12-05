using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Auth;

namespace MeetLines.Application.Services.Interfaces
{
    public interface ITransferUseCases
    {
        Task<Result<string>> CreateTransferAsync(CreateTransferRequest request, string userId, CancellationToken ct = default);
        Task<Result<LoginResponse>> AcceptTransferAsync(AcceptTransferRequest request, CancellationToken ct = default);
    }
}
