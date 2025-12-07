using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Feedback de clientes después del servicio
    /// </summary>
    public class CustomerFeedback
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public int? AppointmentId { get; set; }
        
        /// <summary>Número de WhatsApp del cliente</summary>
        public string CustomerPhone { get; set; } = string.Empty;
        
        /// <summary>Nombre del cliente</summary>
        public string? CustomerName { get; set; }
        
        /// <summary>Rating (1-5)</summary>
        public int Rating { get; set; }
        
        /// <summary>Comentario del cliente</summary>
        public string? Comment { get; set; }
        
        /// <summary>Sentimiento del comentario (-1 a 1)</summary>
        public double? Sentiment { get; set; }
        
        /// <summary>Se notificó al dueño (si es negativo)</summary>
        public bool OwnerNotified { get; set; } = false;
        
        /// <summary>Respuesta del dueño al feedback</summary>
        public string? OwnerResponse { get; set; }
        
        /// <summary>Fecha de respuesta del dueño</summary>
        public DateTime? OwnerRespondedAt { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation
        public Project? Project { get; set; }
        public Appointment? Appointment { get; set; }
    }
}
