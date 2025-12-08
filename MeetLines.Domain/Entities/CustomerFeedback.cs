using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Feedback de clientes después del servicio
    /// </summary>
    public class CustomerFeedback
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public int? AppointmentId { get; private set; }
        
        /// <summary>Número de WhatsApp del cliente</summary>
        public string CustomerPhone { get; private set; }
        
        /// <summary>Nombre del cliente</summary>
        public string? CustomerName { get; private set; }
        
        /// <summary>Rating (1-5)</summary>
        public int Rating { get; private set; }
        
        /// <summary>Comentario del cliente</summary>
        public string? Comment { get; private set; }
        
        /// <summary>Sentimiento del comentario (-1 a 1)</summary>
        public double? Sentiment { get; private set; }
        
        /// <summary>Se notificó al dueño (si es negativo)</summary>
        public bool OwnerNotified { get; private set; }
        
        /// <summary>Respuesta del dueño al feedback</summary>
        public string? OwnerResponse { get; private set; }
        
        /// <summary>Fecha de respuesta del dueño</summary>
        public DateTime? OwnerRespondedAt { get; private set; }
        
        public DateTime CreatedAt { get; private set; }

        // EF Core constructor
        private CustomerFeedback() 
        { 
            CustomerPhone = null!;
        }

        public CustomerFeedback(
            Guid projectId,
            string customerPhone,
            int rating,
            int? appointmentId = null,
            string? customerName = null,
            string? comment = null,
            double? sentiment = null)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(customerPhone)) throw new ArgumentException("CustomerPhone cannot be empty", nameof(customerPhone));
            if (rating < 1 || rating > 5) throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            AppointmentId = appointmentId;
            CustomerPhone = customerPhone;
            CustomerName = customerName;
            Rating = rating;
            Comment = comment;
            Sentiment = sentiment;
            OwnerNotified = false;
            OwnerResponse = null;
            OwnerRespondedAt = null;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkOwnerAsNotified()
        {
            OwnerNotified = true;
        }

        public void AddOwnerResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response)) throw new ArgumentException("Response cannot be empty", nameof(response));
            
            OwnerResponse = response;
            OwnerRespondedAt = DateTime.UtcNow;
        }

        public void UpdateSentiment(double sentiment)
        {
            Sentiment = sentiment;
        }
    }
}
