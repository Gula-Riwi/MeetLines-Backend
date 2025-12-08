using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Configuración del sistema de bots de WhatsApp por proyecto
    /// Usa JSON para configuraciones flexibles
    /// </summary>
    public class ProjectBotConfig
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        
        // ==========================================
        // CONFIGURACIÓN BÁSICA (Columnas)
        // ==========================================
        
        /// <summary>Nombre del bot</summary>
        public string BotName { get; set; } = "Asistente Virtual";
        
        /// <summary>Industria del negocio</summary>
        public string Industry { get; set; } = "general";
        
        /// <summary>Tono de comunicación</summary>
        public string Tone { get; set; } = "friendly";
        
        /// <summary>Zona horaria</summary>
        public string Timezone { get; set; } = "America/Bogota";
        
        // ==========================================
        // CONFIGURACIONES FLEXIBLES (JSON)
        // ==========================================
        
        /// <summary>
        /// Configuración del Bot de Recepción en JSON
        /// Ejemplo: { "enabled": true, "welcomeMessage": "...", "intentKeywords": "..." }
        /// </summary>
        public string ReceptionConfigJson { get; set; } = "{}";
        
        /// <summary>
        /// Configuración del Bot Transaccional en JSON
        /// Ejemplo: { "enabled": true, "appointmentDuration": 60, "bufferMinutes": 0 }
        /// </summary>
        public string TransactionalConfigJson { get; set; } = "{}";
        
        /// <summary>
        /// Configuración del Bot de Feedback en JSON
        /// Ejemplo: { "enabled": true, "delayHours": 24, "googleReviewUrl": "..." }
        /// </summary>
        public string FeedbackConfigJson { get; set; } = "{}";
        
        /// <summary>
        /// Configuración del Bot de Reactivación en JSON
        /// Ejemplo: { "enabled": true, "delayDays": 30, "offerDiscount": false }
        /// </summary>
        public string ReactivationConfigJson { get; set; } = "{}";
        
        /// <summary>
        /// Configuración de Integraciones en JSON
        /// Ejemplo: { "payments": { "enabled": false, "provider": "stripe" } }
        /// </summary>
        public string IntegrationsConfigJson { get; set; } = "{}";
        
        /// <summary>
        /// Configuración Avanzada en JSON
        /// Ejemplo: { "humanFallback": true, "multiAgent": false }
        /// </summary>
        public string AdvancedConfigJson { get; set; } = "{}";
        
        // ==========================================
        // METADATOS
        // ==========================================
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
