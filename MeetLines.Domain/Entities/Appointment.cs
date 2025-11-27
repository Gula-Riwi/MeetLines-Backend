using System;

namespace MeetLines.Domain.Entities
{
    public class Appointment
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public Guid? LeadId { get; private set; }
        public DateTimeOffset StartTime { get; private set; }
        public DateTimeOffset? EndTime { get; private set; }
        public string? MeetingLink { get; private set; }
        public string Status { get; private set; }
        public string? Notes { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private Appointment() { } // EF Core

        public Appointment(Guid projectId, Guid? leadId, DateTimeOffset startTime, DateTimeOffset? endTime)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            LeadId = leadId;
            StartTime = startTime;
            EndTime = endTime;
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
            Notes = reason;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
