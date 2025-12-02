using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Enums;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;

namespace MeetLines.Infrastructure.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public PasswordResetTokenRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token, ct);
        }

        public async Task<PasswordResetToken?> GetLatestByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.PasswordResetTokens
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        public async Task AddAsync(PasswordResetToken token, CancellationToken ct = default)
        {
            await _context.PasswordResetTokens.AddAsync(token, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(PasswordResetToken token, CancellationToken ct = default)
        {
            _context.PasswordResetTokens.Update(token);
            await _context.SaveChangesAsync(ct);
        }

        public async Task InvalidateAllUserTokensAsync(Guid userId, CancellationToken ct = default)
        {
            var tokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && t.Status == PasswordResetTokenStatus.Active)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.MarkAsExpired();
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}