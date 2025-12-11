namespace MeetLines.Application.DTOs.Projects
{
    /// <summary>
    /// DTO para crear un nuevo proyecto/empresa
    /// </summary>
    public class CreateProjectRequest
    {
        public string Name { get; set; } = string.Empty;

        public string? Industry { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
