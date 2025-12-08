using System;
using System.Collections.Generic;

namespace MeetLines.Application.DTOs.Appointments
{
    public class AvailableSlotDto
    {
        public string Time { get; set; } = string.Empty; // "10:00"
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? EmployeeRole { get; set; }
    }

    public class AvailableSlotsResponse
    {
        public string Date { get; set; } = string.Empty; // "2025-12-10"
        public List<AvailableSlotDto> Slots { get; set; } = new();
    }
}
