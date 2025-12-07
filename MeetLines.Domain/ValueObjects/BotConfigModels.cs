using System.Collections.Generic;

namespace MeetLines.Domain.ValueObjects
{
    /// <summary>
    /// Configuración del Bot de Recepción
    /// </summary>
    public class ReceptionBotConfig
    {
        public bool Enabled { get; set; } = true;
        public string WelcomeMessage { get; set; } = "¡Hola! Soy {botName}, el asistente virtual de {businessName}. ¿En qué puedo ayudarte?";
        public string IntentTriggerKeywords { get; set; } = "agendar,reservar,cita,comprar";
        public string HandoffMessage { get; set; } = "¡Perfecto! Te ayudo con eso enseguida 📅";
        public string OutOfHoursMessage { get; set; } = "Gracias por contactarnos. Nuestro horario es {hours}. Te responderemos pronto.";
        public string? CustomPrompt { get; set; }
    }
    
    /// <summary>
    /// Configuración del Bot Transaccional
    /// </summary>
    public class TransactionalBotConfig
    {
        public bool Enabled { get; set; } = true;
        public int AppointmentDurationMinutes { get; set; } = 60;
        public int BufferMinutes { get; set; } = 0;
        public int MaxAdvanceBookingDays { get; set; } = 30;
        public int MinAdvanceBookingDays { get; set; } = 0;
        public string ConfirmationMessage { get; set; } = "✅ ¡Listo! Tu cita está confirmada para el {date} a las {time}.";
        public bool SendReminder { get; set; } = true;
        public int ReminderHoursBefore { get; set; } = 24;
        public string ReminderMessage { get; set; } = "Hola {customerName}, te recordamos tu cita mañana a las {time}.";
        public bool AllowCancellation { get; set; } = true;
        public int MinCancellationHours { get; set; } = 24;
        public string? CustomPrompt { get; set; }
    }
    
    /// <summary>
    /// Configuración del Bot de Feedback
    /// </summary>
    public class FeedbackBotConfig
    {
        public bool Enabled { get; set; } = true;
        public int DelayHours { get; set; } = 24;
        public string RequestMessage { get; set; } = "Hola {customerName}, ¿cómo calificarías tu experiencia del 1 al 5?";
        public string NegativeFeedbackMessage { get; set; } = "Lamentamos eso. ¿Qué podemos mejorar?";
        public bool NotifyOwnerOnNegative { get; set; } = true;
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
            "Hola {customerName}, hace {days} días no te vemos. ¿Te gustaría agendar?",
            "Hola {customerName}, ¿cómo has estado? Tenemos disponibilidad esta semana.",
            "Hola {customerName}, te extrañamos. ¿Podemos ayudarte en algo?"
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
