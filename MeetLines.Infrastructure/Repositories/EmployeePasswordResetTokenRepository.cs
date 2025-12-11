using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;

namespace MeetLines.Infrastructure.Repositories
{
    public class EmployeePasswordResetTokenRepository : IEmployeePasswordResetTokenRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public EmployeePasswordResetTokenRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(EmployeePasswordResetToken token, CancellationToken ct = default)
        {
            await _context.EmployeePasswordResetTokens.AddAsync(token, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<EmployeePasswordResetToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            return await _context.EmployeePasswordResetTokens
                .FirstOrDefaultAsync(x => x.Token == token, ct);
        }

        public async Task UpdateAsync(EmployeePasswordResetToken token, CancellationToken ct = default)
        {
            _context.EmployeePasswordResetTokens.Update(token);
            await _context.SaveChangesAsync(ct);
        }

        public async Task InvalidateAllUserTokensAsync(Guid employeeId, CancellationToken ct = default)
        {
            var tokens = await _context.EmployeePasswordResetTokens
                .Where(x => x.EmployeeId == employeeId && x.Status == MeetLines.Domain.Enums.PasswordResetTokenStatus.Active)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.MarkAsExpired();
            }

            if (tokens.Count > 0)
            {
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
