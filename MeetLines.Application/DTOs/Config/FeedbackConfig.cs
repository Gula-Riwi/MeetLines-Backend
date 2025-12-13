namespace MeetLines.Application.DTOs.Config
{
    public class FeedbackConfig
    {
        public bool Enabled { get; set; }
        public int HoursAfterService { get; set; } = 1; // Default 1 hora después
        public string InitialMessage { get; set; } = "Hola {name}, gracias por confiar en nosotros. ¿Cómo calificarías tu experiencia del 1 al 5?";
        
        // Configuración para el manejo de la respuesta (usado por el Bot conversacional)
        public string HighRatingMessage { get; set; } = "¡Nos alegra mucho! Ayúdanos con una reseña en Google aquí: {link}";
        public string LowRatingMessage { get; set; } = "Lamentamos escuchar eso. ¿Qué podemos mejorar para la próxima?";
        public string? GoogleReviewLink { get; set; }
        public int HighRatingThreshold { get; set; } = 4;
    }
}
