using System;

namespace MeetLines.Domain.Entities
{
    public class Webhook
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Event { get; private set; } = null!;
        public string TargetUrl { get; private set; } = null!;
        public bool Active { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private Webhook() { } // EF Core

        public Webhook(Guid projectId, string @event, string targetUrl)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(@event)) throw new ArgumentException("Event cannot be empty", nameof(@event));
            if (string.IsNullOrWhiteSpace(targetUrl)) throw new ArgumentException("TargetUrl cannot be empty", nameof(targetUrl));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            Event = @event;
            TargetUrl = targetUrl;
            Active = true;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void Deactivate()
        {
            Active = false;
        }

        public void Activate()
        {
            Active = true;
        }
    }
}
