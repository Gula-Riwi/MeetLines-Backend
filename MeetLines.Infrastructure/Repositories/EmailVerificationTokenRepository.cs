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
    public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public EmailVerificationTokenRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            return await _context.EmailVerificationTokens
                .FirstOrDefaultAsync(t => t.Token == token, ct);
        }

        public async Task<EmailVerificationToken?> GetLatestByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.EmailVerificationTokens
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        public async Task AddAsync(EmailVerificationToken token, CancellationToken ct = default)
        {
            await _context.EmailVerificationTokens.AddAsync(token, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(EmailVerificationToken token, CancellationToken ct = default)
        {
            _context.EmailVerificationTokens.Update(token);
            await _context.SaveChangesAsync(ct);
        }

        public async Task InvalidateAllUserTokensAsync(Guid userId, CancellationToken ct = default)
        {
            var tokens = await _context.EmailVerificationTokens
                .Where(t => t.UserId == userId && t.Status == EmailVerificationStatus.Pending)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.MarkAsExpired();
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}