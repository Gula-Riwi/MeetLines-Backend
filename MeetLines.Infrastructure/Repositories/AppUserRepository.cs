using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;

namespace MeetLines.Infrastructure.Repositories
{
    public class AppUserRepository : IAppUserRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public AppUserRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email.ToLower(), ct);
        }

        public async Task AddAsync(AppUser appUser, CancellationToken ct = default)
        {
            _context.AppUsers.Add(appUser);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(AppUser appUser, CancellationToken ct = default)
        {
            _context.AppUsers.Update(appUser);
            await _context.SaveChangesAsync(ct);
        }
    }
}
