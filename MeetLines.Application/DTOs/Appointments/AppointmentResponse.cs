using System;

namespace MeetLines.Application.DTOs.Appointments
{
    public class AppointmentResponse
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? UserNotes { get; set; }
        public string? ClientName { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }
        public string? MeetingLink { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
