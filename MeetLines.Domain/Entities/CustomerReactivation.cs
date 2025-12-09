using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Intentos de reactivación de clientes inactivos
    /// </summary>
    public class CustomerReactivation
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        
        /// <summary>Número de WhatsApp del cliente</summary>
        public string CustomerPhone { get; private set; }
        
        /// <summary>Nombre del cliente</summary>
        public string? CustomerName { get; private set; }
        
        /// <summary>Fecha de última visita/compra</summary>
        public DateTimeOffset LastVisitDate { get; private set; }
        
        /// <summary>Días de inactividad</summary>
        public int DaysInactive { get; private set; }
        
        /// <summary>Número de intento (1, 2, 3)</summary>
        public int AttemptNumber { get; private set; }
        
        /// <summary>Mensaje enviado</summary>
        public string MessageSent { get; private set; }
        
        /// <summary>Respondió el cliente</summary>
        public bool CustomerResponded { get; private set; }
        
        /// <summary>Respuesta del cliente</summary>
        public string? CustomerResponse { get; private set; }
        
        /// <summary>Se reactivó (agendó nueva cita)</summary>
        public bool Reactivated { get; private set; }
        
        /// <summary>ID de la nueva cita (si se reactivó)</summary>
        public int? NewAppointmentId { get; private set; }
        
        /// <summary>Se ofreció descuento</summary>
        public bool DiscountOffered { get; private set; }
        
        /// <summary>Porcentaje de descuento ofrecido</summary>
        public int? DiscountPercentage { get; private set; }
        
        public DateTimeOffset CreatedAt { get; private set; }

        // EF Core constructor
        private CustomerReactivation() 
        { 
            CustomerPhone = null!;
            MessageSent = null!;
        }

        public CustomerReactivation(
            Guid projectId,
            string customerPhone,
            DateTimeOffset lastVisitDate,
            int daysInactive,
            int attemptNumber,
            string messageSent,
            string? customerName = null,
            bool discountOffered = false,
            int? discountPercentage = null)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(customerPhone)) throw new ArgumentException("CustomerPhone cannot be empty", nameof(customerPhone));
            if (string.IsNullOrWhiteSpace(messageSent)) throw new ArgumentException("MessageSent cannot be empty", nameof(messageSent));
            if (attemptNumber < 1) throw new ArgumentException("AttemptNumber must be at least 1", nameof(attemptNumber));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            CustomerPhone = customerPhone;
            CustomerName = customerName;
            LastVisitDate = lastVisitDate;
            DaysInactive = daysInactive;
            AttemptNumber = attemptNumber;
            MessageSent = messageSent;
            CustomerResponded = false;
            CustomerResponse = null;
            Reactivated = false;
            NewAppointmentId = null;
            DiscountOffered = discountOffered;
            DiscountPercentage = discountPercentage;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void RecordCustomerResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response)) throw new ArgumentException("Response cannot be empty", nameof(response));
            
            CustomerResponded = true;
            CustomerResponse = response;
        }

        public void MarkAsReactivated(int appointmentId)
        {
            if (appointmentId <= 0) throw new ArgumentException("AppointmentId must be positive", nameof(appointmentId));
            
            Reactivated = true;
            NewAppointmentId = appointmentId;
        }
    }
}
