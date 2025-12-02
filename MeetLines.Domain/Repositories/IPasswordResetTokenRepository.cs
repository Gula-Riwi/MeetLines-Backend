using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task<PasswordResetToken?> GetLatestByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(PasswordResetToken token, CancellationToken ct = default);
        Task UpdateAsync(PasswordResetToken token, CancellationToken ct = default);
        Task InvalidateAllUserTokensAsync(Guid userId, CancellationToken ct = default);
    }
}