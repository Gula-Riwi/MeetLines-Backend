using System.ComponentModel.DataAnnotations;

namespace MeetLines.Application.DTOs.Projects
{
    public class ConfigureTelegramRequest
    {
        [Required]
        public string BotToken { get; set; } = string.Empty;

        public string? BotUsername { get; set; }
        
        public string? CustomWebhookUrl { get; set; }
        
        public string? ForwardWebhook { get; set; }
    }
}
