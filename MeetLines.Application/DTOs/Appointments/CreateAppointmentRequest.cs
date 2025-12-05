using System;

namespace MeetLines.Application.DTOs.Appointments
{
    public class CreateAppointmentRequest
    {
        public Guid ProjectId { get; set; }
        public int ServiceId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string? UserNotes { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
    }
}
