using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.BotSystem;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class KnowledgeBaseService : IKnowledgeBaseService
    {
        private readonly IKnowledgeBaseRepository _repository;

        public KnowledgeBaseService(IKnowledgeBaseRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<KnowledgeBaseDto>> GetByProjectIdAsync(Guid projectId, bool activeOnly = true, CancellationToken ct = default)
        {
            var entries = activeOnly
                ? await _repository.GetActiveByProjectIdAsync(projectId, ct)
                : await _repository.GetByProjectIdAsync(projectId, ct);

            return entries.Select(MapToDto);
        }

        public async Task<IEnumerable<KnowledgeBaseDto>> SearchAsync(SearchKnowledgeBaseRequest request, CancellationToken ct = default)
        {
            var results = await _repository.SearchAsync(request.ProjectId, request.Query, ct);
            
            if (!string.IsNullOrEmpty(request.Category))
            {
                results = results.Where(x => x.Category == request.Category);
            }

            if (request.ActiveOnly)
            {
                results = results.Where(x => x.IsActive);
            }

            return results.Select(MapToDto);
        }

        public async Task<KnowledgeBaseDto> CreateAsync(CreateKnowledgeBaseRequest request, CancellationToken ct = default)
        {
            var entity = new KnowledgeBase(
                projectId: request.ProjectId,
                category: request.Category,
                question: request.Question,
                answer: request.Answer,
                keywords: JsonSerializer.Serialize(request.Keywords ?? new List<string>()),
                priority: request.Priority
            );

            var created = await _repository.CreateAsync(entity, ct);
            return MapToDto(created);
        }

        public async Task<KnowledgeBaseDto> UpdateAsync(Guid id, UpdateKnowledgeBaseRequest request, CancellationToken ct = default)
        {
            var entity = await _repository.GetAsync(id, ct);
            if (entity == null)
            {
                throw new InvalidOperationException($"Knowledge base entry {id} not found");
            }

            // Use domain method to update
            entity.Update(
                category: request.Category,
                question: request.Question,
                answer: request.Answer,
                keywords: request.Keywords != null ? JsonSerializer.Serialize(request.Keywords) : null,
                priority: request.Priority
            );

            // Handle activation/deactivation separately
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                    entity.Activate();
                else
                    entity.Deactivate();
            }

            await _repository.UpdateAsync(entity, ct);
            return MapToDto(entity);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _repository.DeleteAsync(id, ct);
        }

        public async Task IncrementUsageAsync(Guid id, CancellationToken ct = default)
        {
            await _repository.IncrementUsageAsync(id, ct);
        }

        private KnowledgeBaseDto MapToDto(KnowledgeBase entity)
        {
            return new KnowledgeBaseDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                Category = entity.Category,
                Question = entity.Question,
                Answer = entity.Answer,
                Keywords = JsonSerializer.Deserialize<List<string>>(entity.Keywords) ?? new List<string>(),
                Priority = entity.Priority,
                IsActive = entity.IsActive,
                UsageCount = entity.UsageCount,
                LastUsedAt = entity.LastUsedAt,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
