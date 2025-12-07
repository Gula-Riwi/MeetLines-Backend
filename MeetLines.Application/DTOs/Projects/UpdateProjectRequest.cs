namespace MeetLines.Application.DTOs.Projects
{
    /// <summary>
    /// DTO para actualizar un proyecto/empresa
    /// </summary>
    public class UpdateProjectRequest
    {
        public string Name { get; set; } = null!;
        public string? Subdomain { get; set; }
        public string? Industry { get; set; }
        public string? Description { get; set; }
        // WhatsApp integration fields (optional)
        public string? WhatsappVerifyToken { get; set; }
        public string? WhatsappPhoneNumberId { get; set; }
        public string? WhatsappAccessToken { get; set; }
        public string? WhatsappForwardWebhook { get; set; }
    }
}
