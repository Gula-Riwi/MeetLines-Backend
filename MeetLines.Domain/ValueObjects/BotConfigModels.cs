using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MeetLines.Domain.ValueObjects
{
    /// <summary>
    /// Configuración del Bot de Recepción
    /// </summary>
    public class ReceptionBotConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;
        
        [JsonPropertyName("welcomeMessage")]
        public string WelcomeMessage { get; set; } = "¡Hola! Soy {botName}, el asistente virtual. ¿En qué puedo ayudarte?";
        
        [JsonPropertyName("intentTriggerKeywords")]
        public string IntentTriggerKeywords { get; set; } = "agendar,reservar,cita,comprar";
        
        [JsonPropertyName("handoffMessage")]
        public string HandoffMessage { get; set; } = "¡Perfecto! Te ayudo con eso enseguida.";
        
        [JsonPropertyName("outOfHoursMessage")]
        public string OutOfHoursMessage { get; set; } = "Gracias por contactarnos. Nuestro horario de atención ha terminado. Te responderemos pronto.";
        
        [JsonPropertyName("customPrompt")]
        public string? CustomPrompt { get; set; }
    }
    
    /// <summary>
    /// Configuración del Bot Transaccional
    /// </summary>
    public class TransactionalBotConfig
    {
        [JsonPropertyName("appointmentEnabled")]
        public bool Enabled { get; set; } = true;
        
        [JsonPropertyName("slotDuration")]
        public int AppointmentDurationMinutes { get; set; } = 30;
        
        [JsonPropertyName("bufferBetweenAppointments")]
        public int BufferMinutes { get; set; } = 0;
        
        [JsonPropertyName("businessHours")]
        public Dictionary<string, DaySchedule> BusinessHours { get; set; } = new();

        public int MaxAdvanceBookingDays { get; set; } = 30;
        public int MinAdvanceBookingDays { get; set; } = 0;
        public string ConfirmationMessage { get; set; } = "✅ ¡Listo! Tu cita está confirmada.";
        public bool SendReminder { get; set; } = true;
        public int ReminderHoursBefore { get; set; } = 24;
        
        [JsonPropertyName("reminderMessage")]
        public string ReminderMessage { get; set; } = "Hola, te recordamos tu cita mañana.";
        public bool AllowCancellation { get; set; } = true;
        public int MinCancellationHours { get; set; } = 24;
        public string? CustomPrompt { get; set; }
    }
    
    public class DaySchedule
    {
        [JsonPropertyName("start")]
        public string Start { get; set; } = "09:00";
        
        [JsonPropertyName("end")]
        public string End { get; set; } = "18:00";
        
        [JsonPropertyName("closed")]
        public bool Closed { get; set; } = false;
    }

    /// <summary>
    /// Configuración del Bot de Feedback
    /// </summary>
    public class FeedbackBotConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;
        
        [JsonPropertyName("delayHours")]
        public int DelayHours { get; set; } = 24;
        
        [JsonPropertyName("requestMessage")]
        public string RequestMessage { get; set; } = "Hola, ¿cómo calificarías tu experiencia del 1 al 5?";
        
        [JsonPropertyName("negativeFeedbackMessage")]
        public string NegativeFeedbackMessage { get; set; } = "Lamentamos eso. ¿Qué podemos mejorar?";
        
        [JsonPropertyName("notifyOwnerOnNegative")]
        public bool NotifyOwnerOnNegative { get; set; } = true;
        
        [JsonPropertyName("customPrompt")]
        public string? CustomPrompt { get; set; }
    }
    
    /// <summary>
    /// Configuración del Bot de Reactivación
    /// </summary>
    public class ReactivationBotConfig
    {
        public bool Enabled { get; set; } = true;
        public int DelayDays { get; set; } = 30;
        public int MaxAttempts { get; set; } = 3;
        public int DaysBetweenAttempts { get; set; } = 30;
        public List<string> Messages { get; set; } = new()
        {
            "Hola, hace días no te vemos. ¿Te gustaría agendar?",
            "Hola, ¿cómo has estado? Tenemos disponibilidad esta semana.",
            "Hola, te extrañamos. ¿Podemos ayudarte en algo?"
        };
        public bool OfferDiscount { get; set; } = false;
        public int DiscountPercentage { get; set; } = 10;
        public string DiscountMessage { get; set; } = "¡Tenemos un {discount}% de descuento para ti!";
        public string? CustomPrompt { get; set; }
    }
    
    /// <summary>
    /// Configuración de Integraciones
    /// </summary>
    public class IntegrationsConfig
    {
        public PaymentIntegration? Payments { get; set; }
    }
    
    public class PaymentIntegration
    {
        public bool Enabled { get; set; } = false;
        public string? Provider { get; set; } // stripe, mercadopago, wompi
        public bool RequireAdvancePayment { get; set; } = false;
        public int AdvancePaymentPercentage { get; set; } = 50;
    }
    
    /// <summary>
    /// Configuración Avanzada
    /// </summary>
    public class AdvancedBotConfig
    {
        public bool HumanFallback { get; set; } = true;
        public string HumanFallbackKeywords { get; set; } = "hablar con persona,hablar con humano";
        public string HumanFallbackMessage { get; set; } = "Te conecto con un miembro de nuestro equipo.";
        public List<string>? TeamNotificationNumbers { get; set; }
        public bool MultiAgent { get; set; } = false;
        public string AgentAssignmentStrategy { get; set; } = "round-robin";
        public bool TestMode { get; set; } = false;
        public string? TestPhoneNumber { get; set; }
    }
}
