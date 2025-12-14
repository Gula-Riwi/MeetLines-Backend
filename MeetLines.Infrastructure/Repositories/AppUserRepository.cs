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

             var cleanSearch = phone.Replace("+", "").Replace(" ", "").Replace("-", "").Trim();
             var last7 = cleanSearch.Length > 7 ? cleanSearch.Substring(cleanSearch.Length - 7) : cleanSearch;

             // 1. Initial SQL Filter: Fetch candidates that likely match (by checking last 7 digits)
             var candidates = await _context.AppUsers
                .AsNoTracking()
                .Where(x => x.Phone != null && x.Phone.Contains(last7))
                .ToListAsync(ct);

             // 2. Precise In-Memory Matching (Bidirectional + formatting cleanup)
             foreach(var u in candidates)
             {
                 if(string.IsNullOrEmpty(u.Phone)) continue;
                 var dbClean = u.Phone.Replace("+", "").Replace(" ", "").Replace("-", "").Trim();

                 // Match Logic:
                 // A) Exact Match
                 // B) Input ends with DB (e.g. Input: 57300..., DB: 300...)
                 // C) DB ends with Input (e.g. Input: 300..., DB: 57300...)
                 if (dbClean == cleanSearch || 
                     (dbClean.Length >= 7 && cleanSearch.EndsWith(dbClean)) || 
                     (cleanSearch.Length >= 7 && dbClean.EndsWith(cleanSearch)))
                 {
                     return u;
                 }
             }

             return null;
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
