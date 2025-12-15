using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;

namespace MeetLines.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de suscripciones
    /// </summary>
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public SubscriptionRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active", ct);
        }

        public async Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken ct = default)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, ct);
        }

        public async Task AddAsync(Subscription subscription, CancellationToken ct = default)
        {
            await _context.Subscriptions.AddAsync(subscription, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Subscription subscription, CancellationToken ct = default)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync(ct);
        }
    }
}
