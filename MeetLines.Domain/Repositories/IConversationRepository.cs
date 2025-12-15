using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Conversation entity (Port in Hexagonal Architecture)
    /// </summary>
    public interface IConversationRepository
    {
        /// <summary>
        /// Gets conversation by ID
        /// </summary>
        Task<Conversation?> GetAsync(Guid id, CancellationToken ct = default);
        
        /// <summary>
        /// Gets all conversations for a project
        /// </summary>
        Task<IEnumerable<Conversation>> GetByProjectIdAsync(Guid projectId, int skip, int take, CancellationToken ct = default);
        
        /// <summary>
        /// Gets conversations by customer phone
        /// </summary>
        Task<IEnumerable<Conversation>> GetByCustomerPhoneAsync(Guid projectId, string customerPhone, CancellationToken ct = default);
        
        /// <summary>
        /// Gets conversations by bot type
        /// </summary>
        Task<IEnumerable<Conversation>> GetByBotTypeAsync(Guid projectId, string botType, CancellationToken ct = default);
        
        /// <summary>
        /// Gets conversations requiring human attention
        /// </summary>
        Task<IEnumerable<Conversation>> GetRequiringHumanAttentionAsync(Guid projectId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets conversations by date range
        /// </summary>
        Task<IEnumerable<Conversation>> GetByDateRangeAsync(Guid projectId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken ct = default);
        
        /// <summary>
        /// Creates a new conversation
        /// </summary>
        Task<Conversation> CreateAsync(Conversation conversation, CancellationToken ct = default);
        
        /// <summary>
        /// Updates an existing conversation
        /// </summary>
        Task UpdateAsync(Conversation conversation, CancellationToken ct = default);
        
        /// <summary>
        /// Marks conversation as handled by human
        /// </summary>
        Task MarkAsHandledByHumanAsync(Guid id, Guid employeeId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets conversation count by project
        /// </summary>
        Task<int> GetCountByProjectAsync(Guid projectId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets average sentiment for a project
        /// </summary>
        Task<double?> GetAverageSentimentAsync(Guid projectId, DateTimeOffset? startDate = null, CancellationToken ct = default);
        Task<Conversation?> GetLatestByPhoneAsync(Guid projectId, string phone, CancellationToken ct = default);
    }
}
