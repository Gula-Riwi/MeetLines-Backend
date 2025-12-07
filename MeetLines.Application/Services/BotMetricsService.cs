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
    public class BotMetricsService : IBotMetricsService
    {
        private readonly IBotMetricsRepository _metricsRepo;
        private readonly IConversationRepository _conversationRepo;
        private readonly ICustomerFeedbackRepository _feedbackRepo;
        private readonly ICustomerReactivationRepository _reactivationRepo;

        public BotMetricsService(
            IBotMetricsRepository metricsRepo,
            IConversationRepository conversationRepo,
            ICustomerFeedbackRepository feedbackRepo,
            ICustomerReactivationRepository reactivationRepo)
        {
            _metricsRepo = metricsRepo ?? throw new ArgumentNullException(nameof(metricsRepo));
            _conversationRepo = conversationRepo ?? throw new ArgumentNullException(nameof(conversationRepo));
            _feedbackRepo = feedbackRepo ?? throw new ArgumentNullException(nameof(feedbackRepo));
            _reactivationRepo = reactivationRepo ?? throw new ArgumentNullException(nameof(reactivationRepo));
        }

        public async Task<IEnumerable<BotMetricsDto>> GetMetricsAsync(GetMetricsRequest request, CancellationToken ct = default)
        {
            IEnumerable<BotMetrics> metrics;

            if (request.LastNDays.HasValue)
            {
                metrics = await _metricsRepo.GetLastNDaysAsync(request.ProjectId, request.LastNDays.Value, ct);
            }
            else if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                metrics = await _metricsRepo.GetByDateRangeAsync(request.ProjectId, request.StartDate.Value, request.EndDate.Value, ct);
            }
            else
            {
                var latest = await _metricsRepo.GetLatestAsync(request.ProjectId, ct);
                metrics = latest != null ? new[] { latest } : Array.Empty<BotMetrics>();
            }

            return metrics.Select(MapToDto);
        }

        public async Task<BotMetricsSummaryDto> GetSummaryAsync(Guid projectId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
        {
            var summary = await _metricsRepo.GetSummaryAsync(projectId, startDate, endDate, ct);
            
            return new BotMetricsSummaryDto
            {
                TotalConversations = summary.TotalConversations,
                TotalAppointments = summary.TotalAppointments,
                AverageConversionRate = summary.AverageConversionRate,
                AverageFeedbackRating = summary.AverageFeedbackRating,
                TotalReactivations = summary.TotalReactivations,
                AverageReactivationRate = summary.AverageReactivationRate,
                AverageResponseTime = summary.AverageResponseTime,
                AverageCustomerSatisfaction = summary.AverageCustomerSatisfaction
            };
        }

        public async Task<BotMetricsDto> UpsertMetricsAsync(Guid projectId, DateTime date, CancellationToken ct = default)
        {
            // Calculate metrics for the day
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var conversations = await _conversationRepo.GetByDateRangeAsync(projectId, startOfDay, endOfDay, ct);
            var conversationsList = conversations.ToList();

            var totalConversations = conversationsList.Count;
            var botConversations = conversationsList.Count(x => !x.HandledByHuman);
            var humanConversations = conversationsList.Count(x => x.HandledByHuman);

            var avgFeedbackRating = await _feedbackRepo.GetAverageRatingAsync(projectId, startOfDay, ct);
            var reactivations = await _reactivationRepo.GetSuccessfulReactivationsAsync(projectId, startOfDay, ct);
            var reactivationsList = reactivations.ToList();

            var metrics = new BotMetrics
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Date = startOfDay,
                TotalConversations = totalConversations,
                BotConversations = botConversations,
                HumanConversations = humanConversations,
                AppointmentsBooked = 0,
                ConversionRate = 0,
                AverageFeedbackRating = avgFeedbackRating,
                CustomersReactivated = reactivationsList.Count,
                ReactivationRate = 0,
                AverageResponseTime = 0,
                CustomerSatisfactionScore = avgFeedbackRating ?? 0,
                CreatedAt = DateTime.UtcNow
            };

            var upserted = await _metricsRepo.UpsertAsync(metrics, ct);
            return MapToDto(upserted);
        }

        private BotMetricsDto MapToDto(BotMetrics entity)
        {
            return new BotMetricsDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                Date = entity.Date,
                TotalConversations = entity.TotalConversations,
                BotConversations = entity.BotConversations,
                HumanConversations = entity.HumanConversations,
                AppointmentsBooked = entity.AppointmentsBooked,
                ConversionRate = entity.ConversionRate,
                AverageFeedbackRating = entity.AverageFeedbackRating,
                CustomersReactivated = entity.CustomersReactivated,
                ReactivationRate = entity.ReactivationRate,
                AverageResponseTime = entity.AverageResponseTime,
                CustomerSatisfactionScore = entity.CustomerSatisfactionScore,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
