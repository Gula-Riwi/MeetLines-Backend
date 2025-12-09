using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    /// <summary>
    /// Repository interface for CustomerReactivation entity (Port in Hexagonal Architecture)
    /// </summary>
    public interface ICustomerReactivationRepository
    {
        /// <summary>
        /// Gets reactivation by ID
        /// </summary>
        Task<CustomerReactivation?> GetAsync(Guid id, CancellationToken ct = default);
        
        /// <summary>
        /// Gets all reactivations for a project
        /// </summary>
        Task<IEnumerable<CustomerReactivation>> GetByProjectIdAsync(Guid projectId, int skip, int take, CancellationToken ct = default);
        
        /// <summary>
        /// Gets reactivations by customer phone
        /// </summary>
        Task<IEnumerable<CustomerReactivation>> GetByCustomerPhoneAsync(Guid projectId, string customerPhone, CancellationToken ct = default);
        
        /// <summary>
        /// Gets latest reactivation attempt for a customer
        /// </summary>
        Task<CustomerReactivation?> GetLatestByCustomerPhoneAsync(Guid projectId, string customerPhone, CancellationToken ct = default);
        
        /// <summary>
        /// Gets customers eligible for next reactivation attempt
        /// </summary>
        Task<IEnumerable<CustomerReactivation>> GetEligibleForNextAttemptAsync(Guid projectId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets successful reactivations
        /// </summary>
        Task<IEnumerable<CustomerReactivation>> GetSuccessfulReactivationsAsync(Guid projectId, DateTimeOffset? startDate = null, CancellationToken ct = default);
        
        /// <summary>
        /// Creates a new reactivation attempt
        /// </summary>
        Task<CustomerReactivation> CreateAsync(CustomerReactivation reactivation, CancellationToken ct = default);
        
        /// <summary>
        /// Updates an existing reactivation
        /// </summary>
        Task UpdateAsync(CustomerReactivation reactivation, CancellationToken ct = default);
        
        /// <summary>
        /// Marks reactivation as successful
        /// </summary>
        Task MarkAsReactivatedAsync(Guid id, int newAppointmentId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets reactivation rate for a project
        /// </summary>
        Task<double> GetReactivationRateAsync(Guid projectId, DateTimeOffset? startDate = null, CancellationToken ct = default);
        
        /// <summary>
        /// Gets count of reactivation attempts by attempt number
        /// </summary>
        Task<Dictionary<int, int>> GetAttemptDistributionAsync(Guid projectId, CancellationToken ct = default);
    }
}
