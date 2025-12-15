using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.BotSystem;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IBotMetricsService
    {
        Task<IEnumerable<BotMetricsDto>> GetMetricsAsync(GetMetricsRequest request, CancellationToken ct = default);
        Task<BotMetricsSummaryDto> GetSummaryAsync(Guid projectId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, CancellationToken ct = default);
        Task<BotMetricsDto> UpsertMetricsAsync(Guid projectId, DateTimeOffset date, CancellationToken ct = default);
        Task ProcessDailyMetricsForAllProjectsAsync(CancellationToken ct = default);
    }
}
