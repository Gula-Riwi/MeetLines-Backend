using System;

namespace MeetLines.Domain.Entities
{
    public class ProjectPhoto
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Url { get; private set; }
        public string PublicId { get; private set; }
        public bool IsMain { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private ProjectPhoto() { Url = null!; PublicId = null!; }

        public ProjectPhoto(Guid projectId, string url, string publicId, bool isMain = false)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("Url cannot be empty", nameof(url));
            if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentException("PublicId cannot be empty", nameof(publicId));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            Url = url;
            PublicId = publicId;
            IsMain = isMain;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void SetMain(bool isMain)
        {
            IsMain = isMain;
        }
    }
}

