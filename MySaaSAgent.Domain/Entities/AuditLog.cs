using System;

namespace MySaaSAgent.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; private set; }
        public Guid? UserId { get; private set; }
        public string Action { get; private set; }
        public string? Details { get; private set; } // jsonb
        public string? Ip { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private AuditLog() { } // EF Core

        public AuditLog(Guid? userId, string action, string? details, string? ip)
        {
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action cannot be empty", nameof(action));

            Id = Guid.NewGuid();
            UserId = userId;
            Action = action;
            Details = details;
            Ip = ip;
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
