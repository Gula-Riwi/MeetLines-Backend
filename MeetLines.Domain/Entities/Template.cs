using System;

namespace MeetLines.Domain.Entities
{
    public class Template
    {
        public Guid Id { get; private set; }
        public Guid? ProjectId { get; private set; }
        public string? Industry { get; private set; }
        public string Type { get; private set; } = null!;
        public string Content { get; private set; } = null!;
        public string? Variables { get; private set; } // jsonb
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private Template() { } // EF Core

        public Template(Guid? projectId, string type, string content, string? industry = null)
        {
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type cannot be empty", nameof(type));
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be empty", nameof(content));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            Type = type;
            Content = content;
            Industry = industry;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateContent(string content, string? variables)
        {
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be empty", nameof(content));
            Content = content;
            Variables = variables;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
