using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Historial de conversaciones de WhatsApp
    /// </summary>
    public class Conversation
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        
        /// <summary>Número de WhatsApp del cliente</summary>
        public string CustomerPhone { get; private set; }
        
        /// <summary>Nombre del cliente</summary>
        public string? CustomerName { get; private set; }
        
        /// <summary>Mensaje del cliente</summary>
        public string CustomerMessage { get; private set; }
        
        /// <summary>Respuesta del bot</summary>
        public string BotResponse { get; private set; }
        
        /// <summary>Bot que respondió</summary>
        public string BotType { get; private set; }
        
        /// <summary>Intención detectada</summary>
        public string? Intent { get; private set; }
        
        /// <summary>Confianza de la intención (0-1)</summary>
        public double? IntentConfidence { get; private set; }
        
        /// <summary>Sentimiento del mensaje (-1 a 1)</summary>
        public double? Sentiment { get; private set; }
        
        /// <summary>Requiere atención humana</summary>
        public bool RequiresHumanAttention { get; private set; }
        
        /// <summary>Fue atendido por humano</summary>
        public bool HandledByHuman { get; private set; }
        
        /// <summary>ID del empleado que atendió (si aplica)</summary>
        public Guid? HandledByEmployeeId { get; private set; }
        
        /// <summary>Metadata adicional en JSON</summary>
        public string? MetadataJson { get; private set; }
        
        public DateTime CreatedAt { get; private set; }

        // EF Core constructor
        private Conversation() 
        { 
            CustomerPhone = null!;
            CustomerMessage = null!;
            BotResponse = null!;
            BotType = null!;
        }

        public Conversation(
            Guid projectId,
            string customerPhone,
            string customerMessage,
            string botResponse,
            string botType = "reception",
            string? customerName = null,
            string? intent = null,
            double? intentConfidence = null,
            double? sentiment = null,
            string? metadataJson = null)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(customerPhone)) throw new ArgumentException("CustomerPhone cannot be empty", nameof(customerPhone));
            if (string.IsNullOrWhiteSpace(customerMessage)) throw new ArgumentException("CustomerMessage cannot be empty", nameof(customerMessage));
            if (string.IsNullOrWhiteSpace(botResponse)) throw new ArgumentException("BotResponse cannot be empty", nameof(botResponse));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            CustomerPhone = customerPhone;
            CustomerName = customerName;
            CustomerMessage = customerMessage;
            BotResponse = botResponse;
            BotType = botType;
            Intent = intent;
            IntentConfidence = intentConfidence;
            Sentiment = sentiment;
            RequiresHumanAttention = false;
            HandledByHuman = false;
            HandledByEmployeeId = null;
            MetadataJson = metadataJson;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsRequiringHumanAttention()
        {
            RequiresHumanAttention = true;
        }

        public void AssignToEmployee(Guid employeeId)
        {
            if (employeeId == Guid.Empty) throw new ArgumentException("EmployeeId cannot be empty", nameof(employeeId));
            
            HandledByHuman = true;
            HandledByEmployeeId = employeeId;
        }

        public void UpdateIntent(string intent, double confidence)
        {
            Intent = intent;
            IntentConfidence = confidence;
        }

        public void UpdateSentiment(double sentiment)
        {
            Sentiment = sentiment;
        }

        public void UpdateMetadata(string? metadataJson, string? lastMessage = null, string? lastResponse = null)
        {
            MetadataJson = metadataJson;
            if (!string.IsNullOrWhiteSpace(lastMessage))
                CustomerMessage = lastMessage;
            if (!string.IsNullOrWhiteSpace(lastResponse))
                BotResponse = lastResponse;
        }
    }
}
