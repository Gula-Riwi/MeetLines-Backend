namespace MeetLines.Application.DTOs.Projects
{
    /// <summary>
    /// DTO para crear un nuevo proyecto/empresa
    /// </summary>
    public class CreateProjectRequest
    {
        public string Name { get; set; } = null!;
        public string? Industry { get; set; }
        public string? Description { get; set; }
    }
}
