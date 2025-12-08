using System;

namespace MeetLines.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; private set; } // Changed to int (serial)
        public Guid ProjectId { get; private set; }
        public Guid? LeadId { get; private set; } // Restored
        public Guid? AppUserId { get; private set; } // Made nullable - can be set later
        public int ServiceId { get; private set; } // New
        public Guid? EmployeeId { get; private set; } // New: Assigned Employee
        public DateTimeOffset StartTime { get; private set; }
        public DateTimeOffset EndTime { get; private set; } 
        public string Status { get; private set; } = "pending";
        public decimal PriceSnapshot { get; private set; } 
        public string CurrencySnapshot { get; private set; } = "COP"; 
        public string? MeetingLink { get; private set; }
        public string? UserNotes { get; private set; } 
        public string? AdminNotes { get; private set; } 
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private Appointment() { Status = null!; CurrencySnapshot = null!; } // EF Core

        public Appointment(
            Guid projectId, 
            Guid? leadId,
            Guid? appUserId,  // Now nullable
            int serviceId, 
            DateTimeOffset startTime, 
            DateTimeOffset endTime,
            decimal priceSnapshot,
            string currencySnapshot = "COP",
            string? userNotes = null)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            // if (appUserId == Guid.Empty) throw new ArgumentException("AppUserId cannot be empty", nameof(appUserId)); // AppUserId might be optional? SQL says NOT NULL.
            
            ProjectId = projectId;
            LeadId = leadId;
            AppUserId = appUserId;
            ServiceId = serviceId;
            StartTime = startTime;
            EndTime = endTime;
            PriceSnapshot = priceSnapshot;
            CurrencySnapshot = currencySnapshot;
            UserNotes = userNotes;
            Status = "pending";
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Confirm(string meetingLink)
        {
            Status = "confirmed";
            MeetingLink = meetingLink;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Cancel(string reason)
        {
            Status = "cancelled";
            AdminNotes = string.IsNullOrEmpty(AdminNotes) ? reason : $"{AdminNotes}\nCancellation Reason: {reason}";
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void AssignToEmployee(Guid employeeId)
        {
            if (employeeId == Guid.Empty) throw new ArgumentException("EmployeeId cannot be empty", nameof(employeeId));
            EmployeeId = employeeId;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateAdminNotes(string notes)
        {
            AdminNotes = notes;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
