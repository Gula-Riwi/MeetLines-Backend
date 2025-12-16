using Microsoft.AspNetCore.Http;

namespace MeetLines.API.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo proyecto con foto de perfil opcional
    /// </summary>
    public class CreateProjectWithPhotoRequest
    {
        public string Name { get; set; } = string.Empty;
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
    }
}
