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
    /// EF Core implementation of IBotMetricsRepository (Adapter in Hexagonal Architecture)
    /// </summary>
    public class BotMetricsRepository : IBotMetricsRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public BotMetricsRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<BotMetrics?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.BotMetrics
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<BotMetrics?> GetByProjectAndDateAsync(Guid projectId, DateTimeOffset date, CancellationToken ct = default)
        {
            var dateOnly = date.Date;
            return await _context.BotMetrics
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Date == dateOnly, ct);
        }

        public async Task<IEnumerable<BotMetrics>> GetByDateRangeAsync(Guid projectId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken ct = default)
        {
            var startDateOnly = startDate.Date;
            var endDateOnly = endDate.Date;
            
            return await _context.BotMetrics
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.Date >= startDateOnly && x.Date <= endDateOnly)
                .OrderBy(x => x.Date)
                .ToListAsync(ct);
        }

        public async Task<BotMetrics?> GetLatestAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.BotMetrics
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.Date)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IEnumerable<BotMetrics>> GetLastNDaysAsync(Guid projectId, int days, CancellationToken ct = default)
        {
            var startDate = DateTimeOffset.UtcNow.Date.AddDays(-days);
            
            return await _context.BotMetrics
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.Date >= startDate)
                .OrderBy(x => x.Date)
                .ToListAsync(ct);
        }

        public async Task<BotMetrics> UpsertAsync(BotMetrics metrics, CancellationToken ct = default)
        {
            var existing = await _context.BotMetrics
                .FirstOrDefaultAsync(x => x.ProjectId == metrics.ProjectId && x.Date == metrics.Date, ct);
            
            if (existing != null)
            {
                // Update existing using domain method
                existing.UpdateMetrics(
                    totalConversations: metrics.TotalConversations,
                    botConversations: metrics.BotConversations,
                    humanConversations: metrics.HumanConversations,
                    appointmentsBooked: metrics.AppointmentsBooked,
                    conversionRate: metrics.ConversionRate,
                    customersReactivated: metrics.CustomersReactivated,
                    reactivationRate: metrics.ReactivationRate,
                    averageResponseTime: metrics.AverageResponseTime,
                    customerSatisfactionScore: metrics.CustomerSatisfactionScore,
                    averageFeedbackRating: metrics.AverageFeedbackRating
                );
                await _context.SaveChangesAsync(ct);
                return existing;
            }
            else
            {
                // Create new
                _context.BotMetrics.Add(metrics);
                await _context.SaveChangesAsync(ct);
                return metrics;
            }
        }

        public async Task<BotMetrics> CreateAsync(BotMetrics metrics, CancellationToken ct = default)
        {
            _context.BotMetrics.Add(metrics);
            await _context.SaveChangesAsync(ct);
            return metrics;
        }

        public async Task UpdateAsync(BotMetrics metrics, CancellationToken ct = default)
        {
            _context.BotMetrics.Update(metrics);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var metrics = await _context.BotMetrics.FindAsync(new object[] { id }, ct);
            if (metrics != null)
            {
                _context.BotMetrics.Remove(metrics);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<BotMetricsSummary> GetSummaryAsync(Guid projectId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, CancellationToken ct = default)
        {
            var query = _context.BotMetrics
                .Where(x => x.ProjectId == projectId);

            if (startDate.HasValue)
            {
                query = query.Where(x => x.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.Date <= endDate.Value.Date);
            }

            var metrics = await query.ToListAsync(ct);

            if (!metrics.Any())
            {
                return new BotMetricsSummary();
            }

            return new BotMetricsSummary
            {
                TotalConversations = metrics.Sum(x => x.TotalConversations),
                TotalAppointments = metrics.Sum(x => x.AppointmentsBooked),
                AverageConversionRate = metrics.Average(x => (double)x.ConversionRate),
                AverageFeedbackRating = metrics.Where(x => x.AverageFeedbackRating.HasValue)
                    .Average(x => (double?)x.AverageFeedbackRating) ?? 0,
                TotalReactivations = metrics.Sum(x => x.CustomersReactivated),
                AverageReactivationRate = metrics.Average(x => (double)x.ReactivationRate),
                AverageResponseTime = metrics.Average(x => (double)x.AverageResponseTime),
                AverageCustomerSatisfaction = metrics.Average(x => (double)x.CustomerSatisfactionScore)
            };
        }
    }
}
