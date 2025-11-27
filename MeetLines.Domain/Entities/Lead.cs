using System;

namespace MeetLines.Domain.Entities
{
    public class Lead
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string? Name { get; private set; }
        public string? Email { get; private set; }
        public string? Phone { get; private set; }
        public string? Source { get; private set; }
        public string Stage { get; private set; }
        public int Score { get; private set; }
        public string Urgency { get; private set; }
        public DateTimeOffset? LastInteractionAt { get; internal set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private Lead() { } // EF Core

        public Lead(Guid projectId, string? name, string? email, string? phone, string? source)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            Name = name;
            Email = email;
            Phone = phone;
            Source = source;
            Stage = "new";
            Score = 0;
            Urgency = "low";
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateStage(string stage)
        {
            if (string.IsNullOrWhiteSpace(stage)) throw new ArgumentException("Stage cannot be empty", nameof(stage));
            Stage = stage;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateScore(int score, string urgency)
        {
            Score = score;
            Urgency = urgency;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void RecordInteraction()
        {
            LastInteractionAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
