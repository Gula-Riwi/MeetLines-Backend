using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.BotSystem;

namespace MeetLines.Application.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing bot configurations
    /// </summary>
    public interface IProjectBotConfigService
    {
        /// <summary>
        /// Gets bot configuration by project ID
        /// </summary>
        Task<ProjectBotConfigDto?> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
        
        /// <summary>
        /// Creates bot configuration with industry defaults
        /// </summary>
        Task<ProjectBotConfigDto> CreateAsync(CreateProjectBotConfigRequest request, Guid createdBy, CancellationToken ct = default);
        
        /// <summary>
        /// Updates bot configuration
        /// </summary>
        Task<ProjectBotConfigDto> UpdateAsync(Guid projectId, UpdateProjectBotConfigRequest request, Guid updatedBy, CancellationToken ct = default);
        
        /// <summary>
        /// Deletes bot configuration
        /// </summary>
        Task DeleteAsync(Guid projectId, CancellationToken ct = default);
        
        /// <summary>
        /// Gets industry default configuration
        /// </summary>
        ProjectBotConfigDto GetIndustryDefaults(string industry);
    }
}
