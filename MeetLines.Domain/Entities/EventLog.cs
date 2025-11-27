using System;

namespace MeetLines.Domain.Entities
{
    public class EventLog
    {
        public Guid Id { get; private set; }
        public Guid? ProjectId { get; private set; }
        public Guid? LeadId { get; private set; }
        public string EventType { get; private set; }
        public string? Payload { get; private set; } // jsonb
        public bool Processed { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private EventLog() { } // EF Core

        public EventLog(Guid? projectId, Guid? leadId, string eventType, string? payload)
        {
            if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("EventType cannot be empty", nameof(eventType));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            LeadId = leadId;
            EventType = eventType;
            Payload = payload;
            Processed = false;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsProcessed()
        {
            Processed = true;
        }
    }
}
