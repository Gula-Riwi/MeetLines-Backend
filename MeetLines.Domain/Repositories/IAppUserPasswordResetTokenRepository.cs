using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IAppUserPasswordResetTokenRepository
    {
        Task AddAsync(AppUserPasswordResetToken token, CancellationToken ct = default);
        Task<AppUserPasswordResetToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task UpdateAsync(AppUserPasswordResetToken token, CancellationToken ct = default);
        Task InvalidateAllUserTokensAsync(Guid appUserId, CancellationToken ct = default);
    }
}
