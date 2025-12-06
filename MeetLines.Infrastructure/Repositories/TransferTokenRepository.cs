using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;

namespace MeetLines.Infrastructure.Repositories
{
    public class TransferTokenRepository : ITransferTokenRepository
    {
        private readonly MeetLinesPgDbContext _db;

        public TransferTokenRepository(MeetLinesPgDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(TransferToken token, CancellationToken ct = default)
        {
            await _db.TransferTokens.AddAsync(token, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<TransferToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            return await _db.TransferTokens.FirstOrDefaultAsync(t => t.Token == token, ct);
        }

        public async Task UpdateAsync(TransferToken token, CancellationToken ct = default)
        {
            _db.TransferTokens.Update(token);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var t = await _db.TransferTokens.FindAsync(new object[] { id }, ct);
            if (t != null)
            {
                _db.TransferTokens.Remove(t);
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}
