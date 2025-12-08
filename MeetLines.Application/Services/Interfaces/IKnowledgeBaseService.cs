using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.BotSystem;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IKnowledgeBaseService
    {
        Task<IEnumerable<KnowledgeBaseDto>> GetByProjectIdAsync(Guid projectId, bool activeOnly = true, CancellationToken ct = default);
        Task<IEnumerable<KnowledgeBaseDto>> SearchAsync(SearchKnowledgeBaseRequest request, CancellationToken ct = default);
        Task<KnowledgeBaseDto?> SearchBestAsync(SearchKnowledgeBaseRequest request, CancellationToken ct = default);
        Task<KnowledgeBaseDto> CreateAsync(CreateKnowledgeBaseRequest request, CancellationToken ct = default);
        Task<KnowledgeBaseDto> UpdateAsync(Guid id, UpdateKnowledgeBaseRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task IncrementUsageAsync(Guid id, CancellationToken ct = default);
    }
}
