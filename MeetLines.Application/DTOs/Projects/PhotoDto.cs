using System;

namespace MeetLines.Application.DTOs.Projects
{
    public class PhotoDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
