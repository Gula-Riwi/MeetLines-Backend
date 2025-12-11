using System.ComponentModel.DataAnnotations;

namespace MeetLines.Application.DTOs.Projects
{
    /// <summary>
    /// DTO para configurar la integración de Telegram en un proyecto
    /// Similar a cómo se configura WhatsApp pero más simple
    /// </summary>
    public class ConfigureTelegramRequest
    {
        /// <summary>
        /// Token del bot de Telegram obtenido de @BotFather
        /// Formato: 123456789:ABCdefGHIjklMNOpqrsTUVwxyz
        /// Este token es único y se usa para:
        /// 1. Identificar el bot
        /// 2. Llamar a la API de Telegram
        /// 3. Construir la URL del webhook
        /// </summary>
        [Required(ErrorMessage = "El token del bot de Telegram es requerido")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "El token debe tener entre 10 y 100 caracteres")]
        public string BotToken { get; set; } = null!;

        /// <summary>
        /// Nombre de usuario del bot (opcional, solo para referencia)
        /// Formato: @nombredelbot
        /// </summary>
        [StringLength(100, ErrorMessage = "El username no puede exceder 100 caracteres")]
        public string? BotUsername { get; set; }

        /// <summary>
        /// URL de n8n donde se reenviarán los mensajes de Telegram
        /// Ejemplo: http://localhost:5678/webhook/telegram-bot
        /// Si no se proporciona, se usará una URL por defecto basada en la configuración
        /// </summary>
        [StringLength(500, ErrorMessage = "La URL del webhook no puede exceder 500 caracteres")]
        public string? ForwardWebhook { get; set; }

        /// <summary>
        /// URL personalizada del webhook que Telegram llamará (DEBE SER HTTPS)
        /// Si se proporciona, se usa esta URL en lugar de construirla automáticamente
        /// Ejemplo: https://services.meet-lines.com/webhook/telegram/123456:ABC...
        /// Útil para testing cuando estás en local pero quieres usar la URL del VPS
        /// </summary>
        [StringLength(500, ErrorMessage = "La URL personalizada no puede exceder 500 caracteres")]
        public string? CustomWebhookUrl { get; set; }
    }
}
