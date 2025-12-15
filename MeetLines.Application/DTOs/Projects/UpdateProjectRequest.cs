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
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? ProfilePhotoPublicId { get; set; }
        // WhatsApp integration fields (optional)
        public string? WhatsappVerifyToken { get; set; }
        public string? WhatsappPhoneNumberId { get; set; }
        public string? WhatsappAccessToken { get; set; }
        public string? WhatsappForwardWebhook { get; set; }
    }
}
