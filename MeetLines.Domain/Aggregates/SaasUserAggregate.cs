using System;
using System.Collections.Generic;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Aggregates
{
    public sealed class SaasUserAggregate
    {
        public SaasUser User { get; private set; }
        public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();
        public IReadOnlyCollection<LoginSession> Sessions => _sessions.AsReadOnly();

        private readonly List<Subscription> _subscriptions = new();
        private readonly List<LoginSession> _sessions = new();

        public SaasUserAggregate(SaasUser user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public void AddSubscription(Subscription subscription)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (subscription.UserId != User.Id) throw new InvalidOperationException("Subscription user mismatch");
            _subscriptions.Add(subscription);
        }
    }
}
