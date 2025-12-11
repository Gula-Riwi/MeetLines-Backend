using System;

namespace MeetLines.Application.DTOs.Projects
{
    public class ProjectPublicSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
    }
}
