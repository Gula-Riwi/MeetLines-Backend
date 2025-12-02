using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IEmailVerificationTokenRepository
    {
        Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task<EmailVerificationToken?> GetLatestByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(EmailVerificationToken token, CancellationToken ct = default);
        Task UpdateAsync(EmailVerificationToken token, CancellationToken ct = default);
        Task InvalidateAllUserTokensAsync(Guid userId, CancellationToken ct = default);
    }
}