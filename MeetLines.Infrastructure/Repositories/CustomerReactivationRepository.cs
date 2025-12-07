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
    /// <summary>
    /// EF Core implementation of ICustomerReactivationRepository (Adapter in Hexagonal Architecture)
    /// </summary>
    public class CustomerReactivationRepository : ICustomerReactivationRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public CustomerReactivationRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<CustomerReactivation?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.CustomerReactivations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IEnumerable<CustomerReactivation>> GetByProjectIdAsync(Guid projectId, int skip, int take, CancellationToken ct = default)
        {
            return await _context.CustomerReactivations
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<CustomerReactivation>> GetByCustomerPhoneAsync(Guid projectId, string customerPhone, CancellationToken ct = default)
        {
            return await _context.CustomerReactivations
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.CustomerPhone == customerPhone)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<CustomerReactivation?> GetLatestByCustomerPhoneAsync(Guid projectId, string customerPhone, CancellationToken ct = default)
        {
            return await _context.CustomerReactivations
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.CustomerPhone == customerPhone)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IEnumerable<CustomerReactivation>> GetEligibleForNextAttemptAsync(Guid projectId, CancellationToken ct = default)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            
            return await _context.CustomerReactivations
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId 
                    && !x.Reactivated 
                    && x.AttemptNumber < 3
                    && x.CreatedAt <= thirtyDaysAgo)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<CustomerReactivation>> GetSuccessfulReactivationsAsync(Guid projectId, DateTime? startDate = null, CancellationToken ct = default)
        {
            var query = _context.CustomerReactivations
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.Reactivated);

            if (startDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= startDate.Value);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<CustomerReactivation> CreateAsync(CustomerReactivation reactivation, CancellationToken ct = default)
        {
            _context.CustomerReactivations.Add(reactivation);
            await _context.SaveChangesAsync(ct);
            return reactivation;
        }

        public async Task UpdateAsync(CustomerReactivation reactivation, CancellationToken ct = default)
        {
            _context.CustomerReactivations.Update(reactivation);
            await _context.SaveChangesAsync(ct);
        }

        public async Task MarkAsReactivatedAsync(Guid id, int newAppointmentId, CancellationToken ct = default)
        {
            var reactivation = await _context.CustomerReactivations.FindAsync(new object[] { id }, ct);
            if (reactivation != null)
            {
                reactivation.Reactivated = true;
                reactivation.NewAppointmentId = newAppointmentId;
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<double> GetReactivationRateAsync(Guid projectId, DateTime? startDate = null, CancellationToken ct = default)
        {
            var query = _context.CustomerReactivations
                .Where(x => x.ProjectId == projectId);

            if (startDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= startDate.Value);
            }

            var total = await query.CountAsync(ct);
            if (total == 0) return 0;

            var successful = await query.CountAsync(x => x.Reactivated, ct);
            return (double)successful / total * 100;
        }

        public async Task<Dictionary<int, int>> GetAttemptDistributionAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.CustomerReactivations
                .Where(x => x.ProjectId == projectId)
                .GroupBy(x => x.AttemptNumber)
                .Select(g => new { Attempt = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Attempt, x => x.Count, ct);
        }
    }
}
