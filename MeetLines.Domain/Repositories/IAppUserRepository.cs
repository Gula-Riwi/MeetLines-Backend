using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IAppUserRepository
    {
        Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<AppUser?> GetByPhoneAsync(string phone, CancellationToken ct = default);
        Task AddAsync(AppUser appUser, CancellationToken ct = default);
        Task UpdateAsync(AppUser appUser, CancellationToken ct = default);
    }
}
