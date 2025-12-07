using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Intentos de reactivación de clientes inactivos
    /// </summary>
    public class CustomerReactivation
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        
        /// <summary>Número de WhatsApp del cliente</summary>
        public string CustomerPhone { get; set; } = string.Empty;
        
        /// <summary>Nombre del cliente</summary>
        public string? CustomerName { get; set; }
        
        /// <summary>Fecha de última visita/compra</summary>
        public DateTime LastVisitDate { get; set; }
        
        /// <summary>Días de inactividad</summary>
        public int DaysInactive { get; set; }
        
        /// <summary>Número de intento (1, 2, 3)</summary>
        public int AttemptNumber { get; set; }
        
        /// <summary>Mensaje enviado</summary>
        public string MessageSent { get; set; } = string.Empty;
        
        /// <summary>Respondió el cliente</summary>
        public bool CustomerResponded { get; set; } = false;
        
        /// <summary>Respuesta del cliente</summary>
        public string? CustomerResponse { get; set; }
        
        /// <summary>Se reactivó (agendó nueva cita)</summary>
        public bool Reactivated { get; set; } = false;
        
        /// <summary>ID de la nueva cita (si se reactivó)</summary>
        public int? NewAppointmentId { get; set; }
        
        /// <summary>Se ofreció descuento</summary>
        public bool DiscountOffered { get; set; } = false;
        
        /// <summary>Porcentaje de descuento ofrecido</summary>
        public int? DiscountPercentage { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation
        public Project? Project { get; set; }
        public Appointment? NewAppointment { get; set; }
    }
}
