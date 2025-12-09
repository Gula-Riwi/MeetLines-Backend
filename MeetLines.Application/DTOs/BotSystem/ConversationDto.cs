using System;

namespace MeetLines.Application.DTOs.BotSystem
{
    /// <summary>
    /// DTO for conversation
    /// </summary>
    public class ConversationDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string CustomerMessage { get; set; } = string.Empty;
        public string BotResponse { get; set; } = string.Empty;
        public string BotType { get; set; } = "reception";
        public string? Intent { get; set; }
        public double? IntentConfidence { get; set; }
        public double? Sentiment { get; set; }
        public bool RequiresHumanAttention { get; set; }
        public bool HandledByHuman { get; set; }
        public Guid? HandledByEmployeeId { get; set; }
        public string? MetadataJson { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating conversation (from n8n)
    /// </summary>
    public class CreateConversationRequest
    {
        public Guid ProjectId { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string CustomerMessage { get; set; } = string.Empty;
        public string BotResponse { get; set; } = string.Empty;
        public string BotType { get; set; } = "reception";
        public string? Intent { get; set; }
        public double? IntentConfidence { get; set; }
        public double? Sentiment { get; set; }
        public bool RequiresHumanAttention { get; set; } = false;
        public string? MetadataJson { get; set; }
    }

    /// <summary>
    /// DTO for conversation list with pagination
    /// </summary>
    public class ConversationListRequest
    {
        public Guid ProjectId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? BotType { get; set; }
        public bool? RequiresHumanAttention { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }

    /// <summary>
    /// DTO for updating conversation metadata (from n8n Bot 2)
    /// </summary>
    public class UpdateConversationRequest
    {
        public object? Metadata { get; set; }
        public string? LastMessage { get; set; }
        public string? LastResponse { get; set; }
    }
}
