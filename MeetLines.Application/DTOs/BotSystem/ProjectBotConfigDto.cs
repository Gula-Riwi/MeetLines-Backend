using System;
using System.Text.Json.Serialization;
using MeetLines.Domain.ValueObjects;

namespace MeetLines.Application.DTOs.BotSystem
{
    /// <summary>
    /// DTO for creating/updating bot configuration
    /// </summary>
    public class ProjectBotConfigDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("projectId")]
        public Guid ProjectId { get; set; }
        
        [JsonPropertyName("botName")]
        public string BotName { get; set; } = "Asistente Virtual";
        
        [JsonPropertyName("industry")]
        public string Industry { get; set; } = "general";
        
        [JsonPropertyName("tone")]
        public string Tone { get; set; } = "friendly";
        
        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = "America/Bogota";
        
        // Configuraciones como objetos tipados
        [JsonPropertyName("receptionConfig")]
        public ReceptionBotConfig? ReceptionConfig { get; set; }
        
        [JsonPropertyName("transactionalConfig")]
        public TransactionalBotConfig? TransactionalConfig { get; set; }
        
        [JsonPropertyName("feedbackConfig")]
        public FeedbackBotConfig? FeedbackConfig { get; set; }
        
        [JsonPropertyName("reactivationConfig")]
        public ReactivationBotConfig? ReactivationConfig { get; set; }
        
        [JsonPropertyName("integrationsConfig")]
        public IntegrationsConfig? IntegrationsConfig { get; set; }
        
        [JsonPropertyName("advancedConfig")]
        public AdvancedBotConfig? AdvancedConfig { get; set; }
        
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;
        
        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating bot configuration
    /// </summary>
    public class CreateProjectBotConfigRequest
    {
        [JsonPropertyName("projectId")]
        public Guid ProjectId { get; set; }
        
        [JsonPropertyName("botName")]
        public string? BotName { get; set; }
        
        [JsonPropertyName("industry")]
        public string Industry { get; set; } = "general";
        
        [JsonPropertyName("tone")]
        public string? Tone { get; set; }
        
        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }
    }

    /// <summary>
    /// DTO for updating bot configuration
    /// </summary>
    public class UpdateProjectBotConfigRequest
    {
        [JsonPropertyName("botName")]
        public string? BotName { get; set; }
        
        [JsonPropertyName("tone")]
        public string? Tone { get; set; }
        
        [JsonPropertyName("receptionConfig")]
        public ReceptionBotConfig? ReceptionConfig { get; set; }
        
        [JsonPropertyName("transactionalConfig")]
        public TransactionalBotConfig? TransactionalConfig { get; set; }
        
        [JsonPropertyName("feedbackConfig")]
        public FeedbackBotConfig? FeedbackConfig { get; set; }
        
        [JsonPropertyName("reactivationConfig")]
        public ReactivationBotConfig? ReactivationConfig { get; set; }
        
        [JsonPropertyName("integrationsConfig")]
        public IntegrationsConfig? IntegrationsConfig { get; set; }
        
        [JsonPropertyName("advancedConfig")]
        public AdvancedBotConfig? AdvancedConfig { get; set; }
    }
}
