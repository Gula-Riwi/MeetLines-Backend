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
    }
}
