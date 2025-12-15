using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.BotSystem;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IConversationService
    {
        Task<IEnumerable<ConversationDto>> GetByProjectIdAsync(ConversationListRequest request, CancellationToken ct = default);
        Task<IEnumerable<ConversationDto>> GetByCustomerPhoneAsync(Guid projectId, string customerPhone, CancellationToken ct = default);
        Task<ConversationDto> CreateAsync(CreateConversationRequest request, CancellationToken ct = default);
        Task MarkAsHandledByHumanAsync(Guid id, Guid employeeId, CancellationToken ct = default);
        Task<double?> GetAverageSentimentAsync(Guid projectId, DateTime? startDate = null, CancellationToken ct = default);
        Task UpdateAsync(Guid id, UpdateConversationRequest request, CancellationToken ct = default);
        Task ReturnToBotAsync(Guid projectId, string customerPhone, CancellationToken ct = default);
    }
}
