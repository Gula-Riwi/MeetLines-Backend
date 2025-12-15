using System;

namespace MeetLines.Application.DTOs.Projects
{
    /// <summary>
    /// DTO para respuesta de proyecto
    /// </summary>
    public class ProjectResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string FullUrl { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        // Expose limited WhatsApp fields in response
        public string? WhatsappPhoneNumberId { get; set; }
        public string? WhatsappForwardWebhook { get; set; }

        public string? TelegramBotToken { get; set; }
        public string? TelegramBotUsername { get; set; }
        public string? TelegramForwardWebhook { get; set; }
    }
}
