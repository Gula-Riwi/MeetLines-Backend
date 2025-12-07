using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Historial de conversaciones de WhatsApp
    /// </summary>
    public class Conversation
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        
        /// <summary>Número de WhatsApp del cliente</summary>
        public string CustomerPhone { get; set; } = string.Empty;
        
        /// <summary>Nombre del cliente</summary>
        public string? CustomerName { get; set; }
        
        /// <summary>Mensaje del cliente</summary>
        public string CustomerMessage { get; set; } = string.Empty;
        
        /// <summary>Respuesta del bot</summary>
        public string BotResponse { get; set; } = string.Empty;
        
        /// <summary>Bot que respondió</summary>
        public string BotType { get; set; } = "reception"; // reception, transactional, feedback, reactivation
        
        /// <summary>Intención detectada</summary>
        public string? Intent { get; set; } // info, booking, purchase, complaint
        
        /// <summary>Confianza de la intención (0-1)</summary>
        public double? IntentConfidence { get; set; }
        
        /// <summary>Sentimiento del mensaje (-1 a 1)</summary>
        public double? Sentiment { get; set; }
        
        /// <summary>Requiere atención humana</summary>
        public bool RequiresHumanAttention { get; set; } = false;
        
        /// <summary>Fue atendido por humano</summary>
        public bool HandledByHuman { get; set; } = false;
        
        /// <summary>ID del empleado que atendió (si aplica)</summary>
        public Guid? HandledByEmployeeId { get; set; }
        
        /// <summary>Metadata adicional en JSON</summary>
        public string? MetadataJson { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation
        public Project? Project { get; set; }
        public Employee? HandledByEmployee { get; set; }
    }
}
