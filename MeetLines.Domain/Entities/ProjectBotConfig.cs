using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Configuración del sistema de bots de WhatsApp por proyecto
    /// Usa JSON para configuraciones flexibles
    /// </summary>
    public class ProjectBotConfig
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        
        // ==========================================
        // CONFIGURACIÓN BÁSICA (Columnas)
        // ==========================================
        
        /// <summary>Nombre del bot</summary>
        public string BotName { get; private set; }
        
        /// <summary>Industria del negocio</summary>
        public string Industry { get; private set; }
        
        /// <summary>Tono de comunicación</summary>
        public string Tone { get; private set; }
        
        /// <summary>Zona horaria</summary>
        public string Timezone { get; private set; }
        
        // ==========================================
        // CONFIGURACIONES FLEXIBLES (JSON)
        // ==========================================
        
        /// <summary>
        /// Configuración del Bot de Recepción en JSON
        /// Ejemplo: { "enabled": true, "welcomeMessage": "...", "intentKeywords": "..." }
        /// </summary>
        public string ReceptionConfigJson { get; private set; }
        
        /// <summary>
        /// Configuración del Bot Transaccional en JSON
        /// Ejemplo: { "enabled": true, "appointmentDuration": 60, "bufferMinutes": 0 }
        /// </summary>
        public string TransactionalConfigJson { get; private set; }
        
        /// <summary>
        /// Configuración del Bot de Feedback en JSON
        /// Ejemplo: { "enabled": true, "delayHours": 24, "googleReviewUrl": "..." }
        /// </summary>
        public string FeedbackConfigJson { get; private set; }
        
        /// <summary>
        /// Configuración del Bot de Reactivación en JSON
        /// Ejemplo: { "enabled": true, "delayDays": 30, "offerDiscount": false }
        /// </summary>
        public string ReactivationConfigJson { get; private set; }
        
        /// <summary>
        /// Configuración de Integraciones en JSON
        /// Ejemplo: { "payments": { "enabled": false, "provider": "stripe" } }
        /// </summary>
        public string IntegrationsConfigJson { get; private set; }
        
        /// <summary>
        /// Configuración Avanzada en JSON
        /// Ejemplo: { "humanFallback": true, "multiAgent": false }
        /// </summary>
        public string AdvancedConfigJson { get; private set; }
        
        // ==========================================
        // METADATOS
        // ==========================================
        
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        public Guid CreatedBy { get; private set; }
        public Guid? UpdatedBy { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core constructor
        private ProjectBotConfig() 
        { 
            BotName = null!;
            Industry = null!;
            Tone = null!;
            Timezone = null!;
            ReceptionConfigJson = null!;
            TransactionalConfigJson = null!;
            FeedbackConfigJson = null!;
            ReactivationConfigJson = null!;
            IntegrationsConfigJson = null!;
            AdvancedConfigJson = null!;
        }

        public ProjectBotConfig(
            Guid projectId,
            string botName,
            string industry,
            string tone,
            string timezone,
            string receptionConfigJson,
            string transactionalConfigJson,
            string feedbackConfigJson,
            string reactivationConfigJson,
            string integrationsConfigJson,
            string advancedConfigJson,
            Guid createdBy)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(botName)) throw new ArgumentException("BotName cannot be empty", nameof(botName));
            if (string.IsNullOrWhiteSpace(industry)) throw new ArgumentException("Industry cannot be empty", nameof(industry));
            if (createdBy == Guid.Empty) throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            BotName = botName;
            Industry = industry;
            Tone = tone ?? "friendly";
            Timezone = timezone ?? "America/Bogota";
            ReceptionConfigJson = receptionConfigJson ?? "{}";
            TransactionalConfigJson = transactionalConfigJson ?? "{}";
            FeedbackConfigJson = feedbackConfigJson ?? "{}";
            ReactivationConfigJson = reactivationConfigJson ?? "{}";
            IntegrationsConfigJson = integrationsConfigJson ?? "{}";
            AdvancedConfigJson = advancedConfigJson ?? "{}";
            CreatedBy = createdBy;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
            IsActive = true;
        }

        public void UpdateBasicConfig(string? botName, string? tone, Guid updatedBy)
        {
            if (!string.IsNullOrWhiteSpace(botName))
                BotName = botName;
            
            if (!string.IsNullOrWhiteSpace(tone))
                Tone = tone;

            UpdatedBy = updatedBy;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateReceptionConfig(string receptionConfigJson, Guid updatedBy)
        {
            ReceptionConfigJson = receptionConfigJson ?? throw new ArgumentNullException(nameof(receptionConfigJson));
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTransactionalConfig(string transactionalConfigJson, Guid updatedBy)
        {
            TransactionalConfigJson = transactionalConfigJson ?? throw new ArgumentNullException(nameof(transactionalConfigJson));
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateFeedbackConfig(string feedbackConfigJson, Guid updatedBy)
        {
            FeedbackConfigJson = feedbackConfigJson ?? throw new ArgumentNullException(nameof(feedbackConfigJson));
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateReactivationConfig(string reactivationConfigJson, Guid updatedBy)
        {
            ReactivationConfigJson = reactivationConfigJson ?? throw new ArgumentNullException(nameof(reactivationConfigJson));
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateIntegrationsConfig(string integrationsConfigJson, Guid updatedBy)
        {
            IntegrationsConfigJson = integrationsConfigJson ?? throw new ArgumentNullException(nameof(integrationsConfigJson));
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateAdvancedConfig(string advancedConfigJson, Guid updatedBy)
        {
            AdvancedConfigJson = advancedConfigJson ?? throw new ArgumentNullException(nameof(advancedConfigJson));
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
