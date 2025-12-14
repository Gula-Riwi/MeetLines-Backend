namespace MeetLines.Application.DTOs.Config
{
    public class FeedbackConfig
    {
        public bool Enabled { get; set; }

        public int DelayHours { get; set; } = 1; 
        public string? CustomPrompt { get; set; }
        public string RequestMessage { get; set; } = "Hola {customerName}, ¿cómo calificarías tu experiencia del 1 al 5?";
        public bool NotifyOwnerOnNegative { get; set; }
        public string NegativeFeedbackMessage { get; set; } = "Lamentamos eso. ¿Qué podemos mejorar?";
        
        // Mantener propiedades adicionales que puedan ser útiles aunque no estén en el JSON actual del usuario (opcionales)
        public string? GoogleReviewLink { get; set; }
        public int HighRatingThreshold { get; set; } = 4;
        public string HighRatingMessage { get; set; } = "¡Gracias!";
    }
}
