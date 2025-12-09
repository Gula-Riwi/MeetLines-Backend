using System;
using MeetLines.Domain.ValueObjects;

namespace MeetLines.Application.DTOs.BotSystem
{
    /// <summary>
    /// DTO for creating/updating bot configuration
    /// </summary>
    public class ProjectBotConfigDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string BotName { get; set; } = "Asistente Virtual";
        public string Industry { get; set; } = "general";
        public string Tone { get; set; } = "friendly";
        public string Timezone { get; set; } = "America/Bogota";
        
        // Configuraciones como objetos tipados
        public ReceptionBotConfig? ReceptionConfig { get; set; }
        public TransactionalBotConfig? TransactionalConfig { get; set; }
        public FeedbackBotConfig? FeedbackConfig { get; set; }
        public ReactivationBotConfig? ReactivationConfig { get; set; }
        public IntegrationsConfig? IntegrationsConfig { get; set; }
        public AdvancedBotConfig? AdvancedConfig { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating bot configuration
    /// </summary>
    public class CreateProjectBotConfigRequest
    {
        public Guid ProjectId { get; set; }
        public string? BotName { get; set; }
        public string Industry { get; set; } = "general";
        public string? Tone { get; set; }
        public string? Timezone { get; set; }
    }

    /// <summary>
    /// DTO for updating bot configuration
    /// </summary>
    public class UpdateProjectBotConfigRequest
    {
        public string? BotName { get; set; }
        public string? Tone { get; set; }
        public ReceptionBotConfig? ReceptionConfig { get; set; }
        public TransactionalBotConfig? TransactionalConfig { get; set; }
        public FeedbackBotConfig? FeedbackConfig { get; set; }
        public ReactivationBotConfig? ReactivationConfig { get; set; }
        public IntegrationsConfig? IntegrationsConfig { get; set; }
        public AdvancedBotConfig? AdvancedConfig { get; set; }
    }
}
