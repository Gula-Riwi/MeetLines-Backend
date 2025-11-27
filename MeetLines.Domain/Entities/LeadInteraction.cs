using System;

namespace MeetLines.Domain.Entities
{
    public class LeadInteraction
    {
        public Guid Id { get; private set; }
        public Guid LeadId { get; private set; }
        public string Sender { get; private set; } // lead | bot | human
        public string Channel { get; private set; } // whatsapp | instagram | email | web_form
        public string? Message { get; private set; }
        public string? Metadata { get; private set; } // jsonb
        public DateTimeOffset Timestamp { get; private set; }

        private LeadInteraction() { } // EF Core

        public LeadInteraction(Guid leadId, string sender, string channel, string? message, string? metadata)
        {
            if (leadId == Guid.Empty) throw new ArgumentException("LeadId cannot be empty", nameof(leadId));
            if (string.IsNullOrWhiteSpace(sender)) throw new ArgumentException("Sender cannot be empty", nameof(sender));
            if (string.IsNullOrWhiteSpace(channel)) throw new ArgumentException("Channel cannot be empty", nameof(channel));

            Id = Guid.NewGuid();
            LeadId = leadId;
            Sender = sender;
            Channel = channel;
            Message = message;
            Metadata = metadata;
            Timestamp = DateTimeOffset.UtcNow;
        }
    }
}
