using System;

namespace MeetLines.Domain.Entities
{
    public class Channel
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Type { get; private set; } = null!;
        public string? Credentials { get; private set; } // jsonb
        public bool Verified { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private Channel() { } // EF Core

        public Channel(Guid projectId, string type, string? credentials)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type cannot be empty", nameof(type));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            Type = type;
            Credentials = credentials;
            Verified = false;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Verify()
        {
            Verified = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateCredentials(string credentials)
        {
            Credentials = credentials;
            Verified = false; // Re-verify needed
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
