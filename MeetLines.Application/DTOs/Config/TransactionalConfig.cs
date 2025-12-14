using System.Collections.Generic;

namespace MeetLines.Application.DTOs.Config
{
    public class TransactionalConfig
    {
        public string? CustomPrompt { get; set; }
        public bool SendReminder { get; set; }
        public int SlotDuration { get; set; } = 60;
        public Dictionary<string, BusinessHours> BusinessHours { get; set; } = new();
        public string? ReminderMessage { get; set; }
        public bool AllowCancellation { get; set; }
        public bool AppointmentEnabled { get; set; }
        public string? ConfirmationMessage { get; set; }
        public int ReminderHoursBefore { get; set; } = 24;
        public int MinCancellationHours { get; set; } = 24;
        public int MaxAdvanceBookingDays { get; set; } = 30;
        public int MinAdvanceBookingDays { get; set; } = 0;
        public int BufferBetweenAppointments { get; set; } = 0;
        public int MinHoursBeforeBooking { get; set; } = 0;
    }

    public class BusinessHours
    {
        public bool Closed { get; set; }
        public string Start { get; set; } = "09:00";
        public string End { get; set; } = "18:00";
    }
}
