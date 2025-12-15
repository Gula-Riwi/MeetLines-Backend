using System;

namespace MeetLines.Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Plan { get; private set; } = null!;
        public decimal Price { get; private set; }
        public string Status { get; private set; } = null!;
        public DateTime? RenewalDate { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private Subscription() { } // EF Core

        public Subscription(Guid userId, string plan, decimal price)
        {
            if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(plan)) throw new ArgumentException("Plan cannot be empty", nameof(plan));

            Id = Guid.NewGuid();
            UserId = userId;
            Plan = plan;
            Price = price;
            Status = "active";
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void Cancel()
        {
            Status = "cancelled";
        }

        public void Renew(DateTime renewalDate)
        {
            RenewalDate = renewalDate;
            Status = "active";
        }
    }
}
