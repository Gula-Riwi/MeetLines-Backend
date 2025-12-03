using System.Collections.Generic;

namespace MeetLines.Application.DTOs.Projects
{
    /// <summary>
    /// DTO para respuesta con proyectos del usuario e información del plan
    /// </summary>
    public class UserProjectsResponse
    {
        public string Plan { get; set; } = null!;
        public int MaxProjects { get; set; }
        public int CurrentProjects { get; set; }
        public bool CanCreateMore { get; set; }
        public IReadOnlyCollection<ProjectResponse> Projects { get; set; } = new List<ProjectResponse>();
    }
}
