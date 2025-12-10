using System.ComponentModel.DataAnnotations;

namespace MeetLines.Application.DTOs.Projects
{
    public class ConfigureWhatsappRequest
    {
        [Required]
        public string WhatsappVerifyToken { get; set; } = string.Empty;

        [Required]
        public string WhatsappPhoneNumberId { get; set; } = string.Empty;

        [Required]
        public string WhatsappAccessToken { get; set; } = string.Empty;
    }
}
