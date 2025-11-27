using System;

namespace MeetLines.Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string TagName { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private Tag() { } // EF Core

        public Tag(Guid projectId, string tagName)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(tagName)) throw new ArgumentException("TagName cannot be empty", nameof(tagName));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            TagName = tagName;
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
