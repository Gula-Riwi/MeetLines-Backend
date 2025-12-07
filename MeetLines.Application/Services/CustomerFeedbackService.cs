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
    public class CustomerFeedbackService : ICustomerFeedbackService
    {
        private readonly ICustomerFeedbackRepository _repository;

        public CustomerFeedbackService(ICustomerFeedbackRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<CustomerFeedbackDto>> GetByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 50, CancellationToken ct = default)
        {
            var skip = (page - 1) * pageSize;
            var feedbacks = await _repository.GetByProjectIdAsync(projectId, skip, pageSize, ct);
            return feedbacks.Select(MapToDto);
        }

        public async Task<IEnumerable<CustomerFeedbackDto>> GetNegativeUnrespondedAsync(Guid projectId, CancellationToken ct = default)
        {
            var feedbacks = await _repository.GetNegativeUnrespondedAsync(projectId, ct);
            return feedbacks.Select(MapToDto);
        }

        public async Task<CustomerFeedbackDto> CreateAsync(CreateFeedbackRequest request, CancellationToken ct = default)
        {
            var entity = new CustomerFeedback
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                AppointmentId = request.AppointmentId,
                CustomerPhone = request.CustomerPhone,
                CustomerName = request.CustomerName,
                Rating = request.Rating,
                Comment = request.Comment,
                OwnerNotified = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(entity, ct);
            return MapToDto(created);
        }

        public async Task AddOwnerResponseAsync(Guid id, AddOwnerResponseRequest request, CancellationToken ct = default)
        {
            await _repository.AddOwnerResponseAsync(id, request.Response, ct);
        }

        public async Task<FeedbackStatsDto> GetStatsAsync(Guid projectId, CancellationToken ct = default)
        {
            var avgRating = await _repository.GetAverageRatingAsync(projectId, null, ct);
            var distribution = await _repository.GetRatingDistributionAsync(projectId, ct);
            var negativeUnresponded = await _repository.GetNegativeUnrespondedAsync(projectId, ct);

            return new FeedbackStatsDto
            {
                AverageRating = avgRating ?? 0,
                TotalFeedbacks = distribution.Values.Sum(),
                Rating5Count = distribution.GetValueOrDefault(5, 0),
                Rating4Count = distribution.GetValueOrDefault(4, 0),
                Rating3Count = distribution.GetValueOrDefault(3, 0),
                Rating2Count = distribution.GetValueOrDefault(2, 0),
                Rating1Count = distribution.GetValueOrDefault(1, 0),
                NegativeUnrespondedCount = negativeUnresponded.Count()
            };
        }

        private CustomerFeedbackDto MapToDto(CustomerFeedback entity)
        {
            return new CustomerFeedbackDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                AppointmentId = entity.AppointmentId,
                CustomerPhone = entity.CustomerPhone,
                CustomerName = entity.CustomerName,
                Rating = entity.Rating,
                Comment = entity.Comment,
                Sentiment = entity.Sentiment,
                OwnerNotified = entity.OwnerNotified,
                OwnerResponse = entity.OwnerResponse,
                OwnerRespondedAt = entity.OwnerRespondedAt,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
