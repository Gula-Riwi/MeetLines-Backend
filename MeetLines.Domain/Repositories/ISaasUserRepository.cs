using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface ISaasUserRepository
    {
        Task<SaasUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<SaasUser?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task AddAsync(SaasUser user, CancellationToken ct = default);
        Task UpdateAsync(SaasUser user, CancellationToken ct = default);
        
        // Métodos para OAuth
        Task<SaasUser?> GetByExternalProviderIdAsync(string externalProviderId, CancellationToken ct = default);
        Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default);
    }
}