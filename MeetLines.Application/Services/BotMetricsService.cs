using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.BotSystem;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace MeetLines.Application.Services
{
    public class BotMetricsService : IBotMetricsService
    {
        private readonly IBotMetricsRepository _metricsRepo;
        private readonly IConversationRepository _conversationRepo;
        private readonly ICustomerFeedbackRepository _feedbackRepo;
        private readonly ICustomerReactivationRepository _reactivationRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly Microsoft.Extensions.Logging.ILogger<BotMetricsService> _logger;

        public BotMetricsService(
            IBotMetricsRepository metricsRepo,
            IConversationRepository conversationRepo,
            ICustomerFeedbackRepository feedbackRepo,
            ICustomerReactivationRepository reactivationRepo,
            IProjectRepository projectRepo,
            IAppointmentRepository appointmentRepo,
            Microsoft.Extensions.Logging.ILogger<BotMetricsService> logger)
        {
            _metricsRepo = metricsRepo ?? throw new ArgumentNullException(nameof(metricsRepo));
            _conversationRepo = conversationRepo ?? throw new ArgumentNullException(nameof(conversationRepo));
            _feedbackRepo = feedbackRepo ?? throw new ArgumentNullException(nameof(feedbackRepo));
            _reactivationRepo = reactivationRepo ?? throw new ArgumentNullException(nameof(reactivationRepo));
            _projectRepo = projectRepo ?? throw new ArgumentNullException(nameof(projectRepo));
            _appointmentRepo = appointmentRepo ?? throw new ArgumentNullException(nameof(appointmentRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        public async Task<BotMetricsSummaryDto> GetSummaryAsync(Guid projectId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, CancellationToken ct = default)
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

        public async Task<BotMetricsDto> UpsertMetricsAsync(Guid projectId, DateTimeOffset date, CancellationToken ct = default)
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
            
            // Calculate appointments created on this day
            var createdAppointments = await _appointmentRepo.GetByDateRangeAsync(projectId, startOfDay, endOfDay, ct);
            var appointmentsBooked = createdAppointments.Count(); // New appointments created today

            // Calculation of Conversion Rate (Appointments / Total Conversations)
            double conversionRate = 0;
            if (totalConversations > 0)
            {
                conversionRate = (double)appointmentsBooked / totalConversations * 100;
            }

            var metrics = new BotMetrics(
                projectId: projectId,
                date: startOfDay,
                totalConversations: totalConversations,
                botConversations: botConversations,
                humanConversations: humanConversations,
                appointmentsBooked: appointmentsBooked, // Real count
                conversionRate: conversionRate,
                customersReactivated: reactivationsList.Count,
                reactivationRate: 0, // Need total inactive pool to calculate true rate, leaving 0 for now
                averageResponseTime: 0,
                customerSatisfactionScore: avgFeedbackRating ?? 0,
                averageFeedbackRating: avgFeedbackRating
             );

            var upserted = await _metricsRepo.UpsertAsync(metrics, ct);
            return MapToDto(upserted);
        }

        public async Task ProcessDailyMetricsForAllProjectsAsync(CancellationToken ct = default)
        {
            var projects = await _projectRepo.GetAllAsync(ct);
            // Calculate for Yesterday (assuming job runs at midnight or later)
            var dateToProcess = DateTimeOffset.UtcNow.AddDays(-1);

            foreach (var project in projects)
            {
                try 
                {
                    await UpsertMetricsAsync(project.Id, dateToProcess, ct);
                    _logger.LogInformation("Processed daily metrics for Project {ProjectId} on {Date}", project.Id, dateToProcess.Date);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process metrics for Project {ProjectId}", project.Id);
                    // Continue to next project even if one fails
                }
            }
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
