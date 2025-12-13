using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;

namespace MeetLines.Infrastructure.Repositories
{
    public class AppUserPasswordResetTokenRepository : IAppUserPasswordResetTokenRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public AppUserPasswordResetTokenRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(AppUserPasswordResetToken token, CancellationToken ct = default)
        {
            await _context.AppUserPasswordResetTokens.AddAsync(token, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<AppUserPasswordResetToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            return await _context.AppUserPasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token, ct);
        }

        public async Task UpdateAsync(AppUserPasswordResetToken token, CancellationToken ct = default)
        {
            _context.AppUserPasswordResetTokens.Update(token);
            await _context.SaveChangesAsync(ct);
        }

        public async Task InvalidateAllUserTokensAsync(Guid appUserId, CancellationToken ct = default)
        {
            var tokens = await _context.AppUserPasswordResetTokens
                .Where(t => t.AppUserId == appUserId && !t.IsUsed && t.ExpiresAt > DateTimeOffset.UtcNow)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.MarkAsUsed();
            }
            
            if (tokens.Count > 0)
            {
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
