using System;

namespace MySaaSAgent.Domain.Entities
{
    public class Project
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Name { get; private set; }
        public string? Industry { get; private set; }
        public string? Description { get; private set; }
        public string? WorkingHours { get; private set; } // jsonb
        public string? Config { get; private set; } // jsonb
        public string Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private Project() { } // EF Core

        public Project(Guid userId, string name, string? industry = null, string? description = null)
        {
            if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));

            Id = Guid.NewGuid();
            UserId = userId;
            Name = name;
            Industry = industry;
            Description = description;
            Status = "active";
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateDetails(string name, string? industry, string? description)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            Name = name;
            Industry = industry;
            Description = description;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateConfig(string? workingHours, string? config)
        {
            WorkingHours = workingHours;
            Config = config;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
