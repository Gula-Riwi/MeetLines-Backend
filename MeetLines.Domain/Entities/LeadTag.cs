using System;

namespace MeetLines.Domain.Entities
{
    public class LeadTag
    {
        public Guid Id { get; private set; }
        public Guid LeadId { get; private set; }
        public Guid TagId { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private LeadTag() { } // EF Core

        public LeadTag(Guid leadId, Guid tagId)
        {
            if (leadId == Guid.Empty) throw new ArgumentException("LeadId cannot be empty", nameof(leadId));
            if (tagId == Guid.Empty) throw new ArgumentException("TagId cannot be empty", nameof(tagId));

            Id = Guid.NewGuid();
            LeadId = leadId;
            TagId = tagId;
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
