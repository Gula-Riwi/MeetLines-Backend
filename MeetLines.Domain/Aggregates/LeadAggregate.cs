using System;
using System.Collections.Generic;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Aggregates
{
    public sealed class LeadAggregate
    {
        public Lead Lead { get; private set; }
        private readonly List<LeadInteraction> _interactions = new();

        public IReadOnlyList<LeadInteraction> Interactions => _interactions.AsReadOnly();

        public LeadAggregate(Lead lead)
        {
            Lead = lead ?? throw new ArgumentNullException(nameof(lead));
        }

        public void AddInteraction(LeadInteraction interaction)
        {
            if (interaction == null) throw new ArgumentNullException(nameof(interaction));
            if (interaction.LeadId != Lead.Id) throw new InvalidOperationException("Interaction lead mismatch");
            _interactions.Add(interaction);
            Lead.LastInteractionAt = interaction.Timestamp;
            // domain logic: update score, stage, etc.
        }
    }
}
