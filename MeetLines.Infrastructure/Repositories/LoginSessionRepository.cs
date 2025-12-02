using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;

namespace MeetLines.Infrastructure.Repositories
{
    public class LoginSessionRepository : ILoginSessionRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public LoginSessionRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<LoginSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.LoginSessions
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<LoginSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        {
            return await _context.LoginSessions
                .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, ct);
        }

        public async Task<IEnumerable<LoginSession>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.LoginSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task AddAsync(LoginSession session, CancellationToken ct = default)
        {
            await _context.LoginSessions.AddAsync(session, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(LoginSession session, CancellationToken ct = default)
        {
            _context.LoginSessions.Update(session);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var session = await GetByIdAsync(id, ct);
            if (session != null)
            {
                _context.LoginSessions.Remove(session);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task DeleteExpiredSessionsAsync(CancellationToken ct = default)
        {
            var expiredSessions = await _context.LoginSessions
                .Where(s => s.ExpiresAt.HasValue && s.ExpiresAt.Value < DateTimeOffset.UtcNow)
                .ToListAsync(ct);

            _context.LoginSessions.RemoveRange(expiredSessions);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAllUserSessionsAsync(Guid userId, CancellationToken ct = default)
        {
            var sessions = await _context.LoginSessions
                .Where(s => s.UserId == userId)
                .ToListAsync(ct);

            _context.LoginSessions.RemoveRange(sessions);
            await _context.SaveChangesAsync(ct);
        }
    }
}