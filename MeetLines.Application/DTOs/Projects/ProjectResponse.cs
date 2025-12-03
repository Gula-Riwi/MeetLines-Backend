using System;

namespace MeetLines.Application.DTOs.Projects
{
    /// <summary>
    /// DTO para respuesta de proyecto
    /// </summary>
    public class ProjectResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Industry { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
