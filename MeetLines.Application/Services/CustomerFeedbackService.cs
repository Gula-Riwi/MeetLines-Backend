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
    public class CustomerFeedbackService : ICustomerFeedbackService
    {
        private readonly ICustomerFeedbackRepository _repository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppUserRepository _appUserRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ISaasUserRepository _saasUserRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<CustomerFeedbackService> _logger;

        public CustomerFeedbackService(
            ICustomerFeedbackRepository repository,
            IAppointmentRepository appointmentRepository,
            IAppUserRepository appUserRepository,
            IConversationRepository conversationRepository,
            IProjectRepository projectRepository,
            ISaasUserRepository saasUserRepository,
            IEmailService emailService,
            ILogger<CustomerFeedbackService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _appUserRepository = appUserRepository ?? throw new ArgumentNullException(nameof(appUserRepository));
            _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _saasUserRepository = saasUserRepository ?? throw new ArgumentNullException(nameof(saasUserRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            int? linkedAppointmentId = request.AppointmentId;
            string? customerName = request.CustomerName;

            // Smart Auto-Linking Logic
            if (!linkedAppointmentId.HasValue && !string.IsNullOrEmpty(request.CustomerPhone))
            {
                // 1. Find User by Phone
                var user = await _appUserRepository.GetByPhoneAsync(request.CustomerPhone, ct);
                if (user != null)
                {
                    if (string.IsNullOrEmpty(customerName)) customerName = user.FullName;

                    // 2. Find most recent past appointment
                    var projectAppts = await _appointmentRepository.GetByProjectIdAsync(request.ProjectId, ct);
                    
                    var lastAppt = projectAppts
                        .Where(a => a.AppUserId == user.Id && 
                                    a.StartTime < DateTimeOffset.UtcNow &&
                                    (a.Status == "completed" || a.Status == "confirmed"))
                        .OrderByDescending(a => a.StartTime)
                        .FirstOrDefault();

                    if (lastAppt != null)
                    {
                        linkedAppointmentId = lastAppt.Id;
                        // Optional: Check if really recent? e.g. within 7 days. For now, just take the last one.
                    }
                }
            }

            var entity = new CustomerFeedback(
                projectId: request.ProjectId,
                customerPhone: request.CustomerPhone,
                rating: request.Rating,
                appointmentId: linkedAppointmentId,
                customerName: customerName,
                comment: request.Comment
            );

            var created = await _repository.CreateAsync(entity, ct);

            // 3. Auto-Reset Conversation State (Unlock user)
            // Create a new convo record with 'reception' so n8n sees this as the latest state
            if (!string.IsNullOrEmpty(request.CustomerPhone))
            {
                var resetConvo = new Conversation(
                    projectId: request.ProjectId,
                    customerPhone: request.CustomerPhone,
                    customerMessage: "(System Reset - Feedback Received)", 
                    botResponse: "(Feedback Loop Closed)", 
                    botType: "reception", 
                    customerName: request.CustomerName ?? request.CustomerPhone
                );
                await _conversationRepository.CreateAsync(resetConvo, ct);
            }

            // 4. Negative Feedback Alert (Rating <= 3)
            if (request.Rating <= 3)
            {
                try
                {
                    // Fetch Project and Owner
                    var project = await _projectRepository.GetAsync(request.ProjectId, ct);
                    if (project != null)
                    {
                        var owner = await _saasUserRepository.GetByIdAsync(project.UserId, ct);
                        if (owner != null && !string.IsNullOrEmpty(owner.Email))
                        {
                            await _emailService.SendNegativeFeedbackAlertAsync(
                                owner.Email,
                                owner.Name ?? "Propietario",
                                customerName ?? request.CustomerPhone,
                                request.CustomerPhone,
                                request.Rating,
                                request.Comment,
                                project.Name
                            );
                            _logger.LogInformation($"Alert sent to owner {owner.Email} for low rating ({request.Rating})");
                            
                            // Update OwnerNotified flag
                            // Note: entity is already created. We can update it implicitly or explicitly.
                            // Ideally, we passed 'OwnerNotified = true' to constructor if we did this before creating,
                            // but since creating is done, let's leave it simple or do a quick update.
                            // For performance, sticking to just email is fine for now as requested.
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send negative feedback alert email");
                }
            }

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
