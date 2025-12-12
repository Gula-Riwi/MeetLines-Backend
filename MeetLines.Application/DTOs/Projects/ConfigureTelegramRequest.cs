using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MeetLines.Application.DTOs.Projects
{
    public class ConfigureTelegramRequest
    {
        [Required]
        public string BotToken { get; set; } = string.Empty;

        public string? BotUsername { get; set; }
        
        [JsonIgnore]
        public string? CustomWebhookUrl { get; set; }
        
        [JsonIgnore]
        public string? ForwardWebhook { get; set; }
    }
}
