using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.BotSystem;

namespace MeetLines.Application.Services.Interfaces
{
    public interface ICustomerFeedbackService
    {
        Task<IEnumerable<CustomerFeedbackDto>> GetByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 50, CancellationToken ct = default);
        Task<IEnumerable<CustomerFeedbackDto>> GetNegativeUnrespondedAsync(Guid projectId, CancellationToken ct = default);
        Task<CustomerFeedbackDto> CreateAsync(CreateFeedbackRequest request, CancellationToken ct = default);
        Task AddOwnerResponseAsync(Guid id, AddOwnerResponseRequest request, CancellationToken ct = default);
        Task<FeedbackStatsDto> GetStatsAsync(Guid projectId, CancellationToken ct = default);
    }
}
