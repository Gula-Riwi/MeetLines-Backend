using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    /// <summary>
    /// Repository interface for CustomerFeedback entity (Port in Hexagonal Architecture)
    /// </summary>
    public interface ICustomerFeedbackRepository
    {
        /// <summary>
        /// Gets feedback by ID
        /// </summary>
        Task<CustomerFeedback?> GetAsync(Guid id, CancellationToken ct = default);
        
        /// <summary>
        /// Gets all feedback for a project
        /// </summary>
        Task<IEnumerable<CustomerFeedback>> GetByProjectIdAsync(Guid projectId, int skip, int take, CancellationToken ct = default);
        
        /// <summary>
        /// Gets feedback by appointment ID
        /// </summary>
        Task<CustomerFeedback?> GetByAppointmentIdAsync(int appointmentId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets feedback by rating
        /// </summary>
        Task<IEnumerable<CustomerFeedback>> GetByRatingAsync(Guid projectId, int rating, CancellationToken ct = default);
        
        /// <summary>
        /// Gets negative feedback without owner response
        /// </summary>
        Task<IEnumerable<CustomerFeedback>> GetNegativeUnrespondedAsync(Guid projectId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets feedback by date range
        /// </summary>
        Task<IEnumerable<CustomerFeedback>> GetByDateRangeAsync(Guid projectId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken ct = default);
        
        /// <summary>
        /// Creates a new feedback
        /// </summary>
        Task<CustomerFeedback> CreateAsync(CustomerFeedback feedback, CancellationToken ct = default);
        
        /// <summary>
        /// Updates an existing feedback
        /// </summary>
        Task UpdateAsync(CustomerFeedback feedback, CancellationToken ct = default);
        
        /// <summary>
        /// Adds owner response to feedback
        /// </summary>
        Task AddOwnerResponseAsync(Guid id, string response, CancellationToken ct = default);
        
        /// <summary>
        /// Gets average rating for a project
        /// </summary>
        Task<double?> GetAverageRatingAsync(Guid projectId, DateTimeOffset? startDate = null, CancellationToken ct = default);
        
        /// <summary>
        /// Gets feedback count by rating
        /// </summary>
        Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid projectId, CancellationToken ct = default);
    }
}
