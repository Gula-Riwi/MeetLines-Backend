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
    /// EF Core implementation of ICustomerFeedbackRepository (Adapter in Hexagonal Architecture)
    /// </summary>
    public class CustomerFeedbackRepository : ICustomerFeedbackRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public CustomerFeedbackRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<CustomerFeedback?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.CustomerFeedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IEnumerable<CustomerFeedback>> GetByProjectIdAsync(Guid projectId, int skip, int take, CancellationToken ct = default)
        {
            return await _context.CustomerFeedbacks
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<CustomerFeedback?> GetByAppointmentIdAsync(int appointmentId, CancellationToken ct = default)
        {
            return await _context.CustomerFeedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId, ct);
        }

        public async Task<IEnumerable<CustomerFeedback>> GetByRatingAsync(Guid projectId, int rating, CancellationToken ct = default)
        {
            return await _context.CustomerFeedbacks
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.Rating == rating)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<CustomerFeedback>> GetNegativeUnrespondedAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.CustomerFeedbacks
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.Rating <= 3 && x.OwnerResponse == null)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<CustomerFeedback>> GetByDateRangeAsync(Guid projectId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken ct = default)
        {
            return await _context.CustomerFeedbacks
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<CustomerFeedback> CreateAsync(CustomerFeedback feedback, CancellationToken ct = default)
        {
            _context.CustomerFeedbacks.Add(feedback);
            await _context.SaveChangesAsync(ct);
            return feedback;
        }

        public async Task UpdateAsync(CustomerFeedback feedback, CancellationToken ct = default)
        {
            _context.CustomerFeedbacks.Update(feedback);
            await _context.SaveChangesAsync(ct);
        }

        public async Task AddOwnerResponseAsync(Guid id, string response, CancellationToken ct = default)
        {
            var feedback = await _context.CustomerFeedbacks.FindAsync(new object[] { id }, ct);
            if (feedback != null)
            {
                feedback.AddOwnerResponse(response);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<double?> GetAverageRatingAsync(Guid projectId, DateTimeOffset? startDate = null, CancellationToken ct = default)
        {
            var query = _context.CustomerFeedbacks
                .Where(x => x.ProjectId == projectId);

            if (startDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= startDate.Value);
            }

            return await query.AverageAsync(x => (double?)x.Rating, ct);
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.CustomerFeedbacks
                .Where(x => x.ProjectId == projectId)
                .GroupBy(x => x.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count, ct);
        }
    }
}
