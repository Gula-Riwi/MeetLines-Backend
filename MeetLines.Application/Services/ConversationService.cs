using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.BotSystem;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _repository;

        public ConversationService(IConversationRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<ConversationDto>> GetByProjectIdAsync(ConversationListRequest request, CancellationToken ct = default)
        {
            var skip = (request.Page - 1) * request.PageSize;
            var conversations = await _repository.GetByProjectIdAsync(
                request.ProjectId, 
                skip, 
                request.PageSize, 
                request.BotType, 
                request.RequiresHumanAttention, 
                request.AssignedToEmployeeId,
                (request.StartDate ?? DateTime.MinValue) == DateTime.MinValue ? null : request.StartDate, 
                (request.EndDate ?? DateTime.MaxValue) == DateTime.MaxValue ? null : request.EndDate, 
                ct);

            return conversations.Select(MapToDto);
        }

        public async Task<IEnumerable<ConversationDto>> GetByCustomerPhoneAsync(Guid projectId, string customerPhone, CancellationToken ct = default)
        {
            // Normalize to search by last 10 digits to ensure match regardless of country code prefix
            var searchPhone = customerPhone?.Replace("+", "").Replace(" ", "").Trim();
            if (!string.IsNullOrEmpty(searchPhone) && searchPhone.Length > 10)
            {
                searchPhone = searchPhone.Substring(searchPhone.Length - 10);
            }

            var conversations = await _repository.GetByCustomerPhoneAsync(projectId, searchPhone!, ct);
            return conversations.Select(MapToDto);
        }

        public async Task<ConversationDto> CreateAsync(CreateConversationRequest request, CancellationToken ct = default)
        {
            var entity = new Conversation(
                projectId: request.ProjectId,
                customerPhone: request.CustomerPhone,
                customerMessage: request.CustomerMessage,
                botResponse: request.BotResponse,
                botType: request.BotType,
                customerName: request.CustomerName,
                intent: request.Intent,
                intentConfidence: request.IntentConfidence,
                sentiment: request.Sentiment
            );

            // Mark as requiring human attention if requested
            if (request.RequiresHumanAttention)
            {
                entity.MarkAsRequiringHumanAttention();
            }

            // Set initial metadata and messages if provided
            if (!string.IsNullOrWhiteSpace(request.MetadataJson) || !string.IsNullOrWhiteSpace(request.LastMessage))
            {
                entity.UpdateMetadata(request.MetadataJson, request.LastMessage, request.LastResponse);
            }

            var created = await _repository.CreateAsync(entity, ct);
            return MapToDto(created);
        }

        public async Task MarkAsHandledByHumanAsync(Guid id, Guid employeeId, CancellationToken ct = default)
        {
            await _repository.MarkAsHandledByHumanAsync(id, employeeId, ct);
        }

        public async Task<double?> GetAverageSentimentAsync(Guid projectId, DateTime? startDate = null, CancellationToken ct = default)
        {
            return await _repository.GetAverageSentimentAsync(projectId, startDate, ct);
        }

        public async Task UpdateAsync(Guid id, UpdateConversationRequest request, CancellationToken ct = default)
        {
            var conversation = await _repository.GetAsync(id, ct);
            if (conversation == null)
            {
                throw new InvalidOperationException($"Conversation {id} not found");
            }

            var metadataJson = request.Metadata != null 
                ? System.Text.Json.JsonSerializer.Serialize(request.Metadata) 
                : null;

            conversation.UpdateMetadata(metadataJson, request.LastMessage, request.LastResponse);
            await _repository.UpdateAsync(conversation, ct);
        }

        public async Task ReturnToBotAsync(Guid projectId, string customerPhone, CancellationToken ct = default)
        {
            var entity = new Conversation(
                projectId: projectId,
                customerPhone: customerPhone,
                customerMessage: "(Manual Return to Bot)",
                botResponse: "(Bot Active)",
                botType: "reception" // Resets state to 'reception'
            );
            
            // We do NOT mark as handled by human, effectively resetting the 'flag' if logic checks the latest record.
            
            await _repository.CreateAsync(entity, ct);
        }

        private ConversationDto MapToDto(Conversation entity)
        {
            return new ConversationDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                CustomerPhone = entity.CustomerPhone,
                CustomerName = entity.CustomerName,
                CustomerMessage = entity.CustomerMessage,
                BotResponse = entity.BotResponse,
                BotType = entity.BotType,
                Intent = entity.Intent,
                IntentConfidence = entity.IntentConfidence,
                Sentiment = entity.Sentiment,
                RequiresHumanAttention = entity.RequiresHumanAttention,
                HandledByHuman = entity.HandledByHuman,
                HandledByEmployeeId = entity.HandledByEmployeeId,
                MetadataJson = entity.MetadataJson,
                LastMessage = entity.LastMessage,
                LastResponse = entity.LastResponse,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
