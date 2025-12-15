using System;

namespace MeetLines.Application.DTOs.Projects
{
    /// <summary>
    /// DTO para respuesta pública de proyecto (usado por n8n e integraciones)
    /// Solo contiene datos públicos, acceso controlado por INTEGRATIONS_API_KEY
    /// </summary>
    public class ProjectPublicDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Industry { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? ProfilePhotoUrl { get; set; }
    }
}
