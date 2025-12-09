using System;

namespace MeetLines.Application.DTOs.BotSystem
{
    /// <summary>
    /// DTO for customer feedback
    /// </summary>
    public class CustomerFeedbackDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public int? AppointmentId { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public double? Sentiment { get; set; }
        public bool OwnerNotified { get; set; }
        public string? OwnerResponse { get; set; }
        public DateTimeOffset? OwnerRespondedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating feedback (from n8n)
    /// </summary>
    public class CreateFeedbackRequest
    {
        public Guid ProjectId { get; set; }
        public int? AppointmentId { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    /// <summary>
    /// DTO for owner response to feedback
    /// </summary>
    public class AddOwnerResponseRequest
    {
        public string Response { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for feedback statistics
    /// </summary>
    public class FeedbackStatsDto
    {
        public double AverageRating { get; set; }
        public int TotalFeedbacks { get; set; }
        public int Rating5Count { get; set; }
        public int Rating4Count { get; set; }
        public int Rating3Count { get; set; }
        public int Rating2Count { get; set; }
        public int Rating1Count { get; set; }
        public int NegativeUnrespondedCount { get; set; }
    }
}
