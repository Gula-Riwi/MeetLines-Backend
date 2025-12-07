using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    /// <summary>
    /// Repository interface for BotMetrics entity (Port in Hexagonal Architecture)
    /// </summary>
    public interface IBotMetricsRepository
    {
        /// <summary>
        /// Gets metrics by ID
        /// </summary>
        Task<BotMetrics?> GetAsync(Guid id, CancellationToken ct = default);
        
        /// <summary>
        /// Gets metrics by project and date
        /// </summary>
        Task<BotMetrics?> GetByProjectAndDateAsync(Guid projectId, DateTime date, CancellationToken ct = default);
        
        /// <summary>
        /// Gets metrics for a project within date range
        /// </summary>
        Task<IEnumerable<BotMetrics>> GetByDateRangeAsync(Guid projectId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
        
        /// <summary>
        /// Gets latest metrics for a project
        /// </summary>
        Task<BotMetrics?> GetLatestAsync(Guid projectId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets metrics for last N days
        /// </summary>
        Task<IEnumerable<BotMetrics>> GetLastNDaysAsync(Guid projectId, int days, CancellationToken ct = default);
        
        /// <summary>
        /// Creates or updates metrics for a specific date
        /// </summary>
        Task<BotMetrics> UpsertAsync(BotMetrics metrics, CancellationToken ct = default);
        
        /// <summary>
        /// Creates new metrics
        /// </summary>
        Task<BotMetrics> CreateAsync(BotMetrics metrics, CancellationToken ct = default);
        
        /// <summary>
        /// Updates existing metrics
        /// </summary>
        Task UpdateAsync(BotMetrics metrics, CancellationToken ct = default);
        
        /// <summary>
        /// Deletes metrics
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        
        /// <summary>
        /// Gets aggregated metrics summary
        /// </summary>
        Task<BotMetricsSummary> GetSummaryAsync(Guid projectId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
    }
    
    /// <summary>
    /// Summary of bot metrics for a period
    /// </summary>
    public class BotMetricsSummary
    {
        public int TotalConversations { get; set; }
        public int TotalAppointments { get; set; }
        public double AverageConversionRate { get; set; }
        public double AverageFeedbackRating { get; set; }
        public int TotalReactivations { get; set; }
        public double AverageReactivationRate { get; set; }
        public double AverageResponseTime { get; set; }
        public double AverageCustomerSatisfaction { get; set; }
    }
}
