using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    /// <summary>
    /// Repository interface for ProjectBotConfig entity (Port in Hexagonal Architecture)
    /// </summary>
    public interface IProjectBotConfigRepository
    {
        /// <summary>
        /// Gets bot configuration by project ID
        /// </summary>
        Task<ProjectBotConfig?> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets bot configuration by ID
        /// </summary>
        Task<ProjectBotConfig?> GetAsync(Guid id, CancellationToken ct = default);
        
        /// <summary>
        /// Creates a new bot configuration
        /// </summary>
        Task<ProjectBotConfig> CreateAsync(ProjectBotConfig config, CancellationToken ct = default);
        
        /// <summary>
        /// Updates an existing bot configuration
        /// </summary>
        Task UpdateAsync(ProjectBotConfig config, CancellationToken ct = default);
        
        /// <summary>
        /// Deletes a bot configuration
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        
        /// <summary>
        /// Checks if a project has bot configuration
        /// </summary>
        Task<bool> ExistsForProjectAsync(Guid projectId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets all bot configurations by industry
        /// </summary>
        Task<IEnumerable<ProjectBotConfig>> GetByIndustryAsync(string industry, CancellationToken ct = default);
    }
}
