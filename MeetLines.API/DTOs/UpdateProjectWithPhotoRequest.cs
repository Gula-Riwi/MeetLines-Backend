using Microsoft.AspNetCore.Http;

namespace MeetLines.API.DTOs
{
    /// <summary>
    /// DTO para actualizar un proyecto con foto de perfil opcional
    /// </summary>
    public class UpdateProjectWithPhotoRequest
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
        
        /// <summary>
        /// Archivo de foto de perfil opcional
        /// </summary>
        [Microsoft.AspNetCore.Mvc.FromForm(Name = "profilePhoto")]
        public IFormFile? ProfilePhoto { get; set; }
        
        // WhatsApp integration fields (optional)
        public string? WhatsappVerifyToken { get; set; }
        public string? WhatsappPhoneNumberId { get; set; }
        public string? WhatsappAccessToken { get; set; }
        public string? WhatsappForwardWebhook { get; set; }
    }
}
