using System;

namespace MeetLines.Application.DTOs.Appointments
{
    public class AppointmentResponse
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public int ServiceId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? UserNotes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
