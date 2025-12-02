using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface ILoginSessionRepository
    {
        Task<LoginSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<LoginSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
        Task<IEnumerable<LoginSession>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(LoginSession session, CancellationToken ct = default);
        Task UpdateAsync(LoginSession session, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task DeleteExpiredSessionsAsync(CancellationToken ct = default);
        Task DeleteAllUserSessionsAsync(Guid userId, CancellationToken ct = default);
    }
}