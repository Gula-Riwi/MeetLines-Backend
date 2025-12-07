using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    /// <summary>
    /// Repository interface for KnowledgeBase entity (Port in Hexagonal Architecture)
    /// </summary>
    public interface IKnowledgeBaseRepository
    {
        /// <summary>
        /// Gets knowledge base entry by ID
        /// </summary>
        Task<KnowledgeBase?> GetAsync(Guid id, CancellationToken ct = default);
        
        /// <summary>
        /// Gets all knowledge base entries for a project
        /// </summary>
        Task<IEnumerable<KnowledgeBase>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets active knowledge base entries for a project
        /// </summary>
        Task<IEnumerable<KnowledgeBase>> GetActiveByProjectIdAsync(Guid projectId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets knowledge base entries by category
        /// </summary>
        Task<IEnumerable<KnowledgeBase>> GetByCategoryAsync(Guid projectId, string category, CancellationToken ct = default);
        
        /// <summary>
        /// Searches knowledge base entries by keywords
        /// </summary>
        Task<IEnumerable<KnowledgeBase>> SearchAsync(Guid projectId, string query, CancellationToken ct = default);
        
        /// <summary>
        /// Creates a new knowledge base entry
        /// </summary>
        Task<KnowledgeBase> CreateAsync(KnowledgeBase knowledgeBase, CancellationToken ct = default);
        
        /// <summary>
        /// Updates an existing knowledge base entry
        /// </summary>
        Task UpdateAsync(KnowledgeBase knowledgeBase, CancellationToken ct = default);
        
        /// <summary>
        /// Deletes a knowledge base entry
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        
        /// <summary>
        /// Increments usage count for a knowledge base entry
        /// </summary>
        Task IncrementUsageAsync(Guid id, CancellationToken ct = default);
        
        /// <summary>
        /// Gets most used knowledge base entries
        /// </summary>
        Task<IEnumerable<KnowledgeBase>> GetMostUsedAsync(Guid projectId, int count, CancellationToken ct = default);
    }
}
