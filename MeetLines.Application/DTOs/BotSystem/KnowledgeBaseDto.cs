using System;
using System.Collections.Generic;

namespace MeetLines.Application.DTOs.BotSystem
{
    /// <summary>
    /// DTO for knowledge base entry
    /// </summary>
    public class KnowledgeBaseDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Category { get; set; } = "general";
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new();
        public int Priority { get; set; }
        public bool IsActive { get; set; } = true;
        public int UsageCount { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating knowledge base entry
    /// </summary>
    public class CreateKnowledgeBaseRequest
    {
        public Guid ProjectId { get; set; }
        public string Category { get; set; } = "general";
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public List<string>? Keywords { get; set; }
        public int Priority { get; set; } = 0;
    }

    /// <summary>
    /// DTO for updating knowledge base entry
    /// </summary>
    public class UpdateKnowledgeBaseRequest
    {
        public string? Category { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public List<string>? Keywords { get; set; }
        public int? Priority { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for searching knowledge base
    /// </summary>
    public class SearchKnowledgeBaseRequest
    {
        public Guid ProjectId { get; set; }
        public string Query { get; set; } = string.Empty;
        public string? Category { get; set; }
        public bool ActiveOnly { get; set; } = true;
    }
}
