using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.AiInsights;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IAiInsightsService
    {
        /// <summary>
        /// Generates a consolidated AI insights report for the project dashboard.
        /// </summary>
        Task<AiInsightsDto> GetProjectInsightsAsync(Guid projectId, CancellationToken ct = default);
    }
}
