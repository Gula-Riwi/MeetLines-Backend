using System;

namespace MeetLines.Application.DTOs.Appointments
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "COP";
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }
}
