using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;

namespace MeetLines.Infrastructure.Repositories
{
    public class SaasUserRepository : ISaasUserRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public SaasUserRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<SaasUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.SaasUsers
                .FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<SaasUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _context.SaasUsers
                .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);
        }

        public async Task<SaasUser?> GetByExternalProviderIdAsync(string externalProviderId, CancellationToken ct = default)
        {
            return await _context.SaasUsers
                .FirstOrDefaultAsync(u => u.ExternalProviderId == externalProviderId, ct);
        }

        public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default)
        {
            return await _context.SaasUsers
                .AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);
        }

        public async Task AddAsync(SaasUser user, CancellationToken ct = default)
        {
            await _context.SaasUsers.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(SaasUser user, CancellationToken ct = default)
        {
            _context.SaasUsers.Update(user);
            await _context.SaveChangesAsync(ct);
        }
    }
}