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

        public async Task<AppUser?> GetByPhoneAsync(string phone, CancellationToken ct = default)
        {
             if (string.IsNullOrWhiteSpace(phone)) return null;

             // Normalize: Remove +, spaces, dashes
             var cleanSearch = phone.Replace("+", "").Replace(" ", "").Replace("-", "").Trim();
             
             // Strategy: 
             // 1. Exact match (fastest)
             // 2. EndsWith (to match local number input against stored international format)
             //    Only apply EndsWith if search term is reasonably unique (> 6 digits)
             
             return await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Phone.Contains(cleanSearch) || (cleanSearch.Length > 6 && x.Phone.EndsWith(cleanSearch)), ct);
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
