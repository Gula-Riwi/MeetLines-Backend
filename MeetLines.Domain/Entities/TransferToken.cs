using System;

namespace MeetLines.Domain.Entities
{
    public class TransferToken
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public string Tenant { get; private set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; private set; }
        public bool Used { get; private set; }

        private TransferToken() { }

        public TransferToken(Guid userId, string token, string tenant, DateTimeOffset expiresAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Token = token;
            Tenant = tenant;
            ExpiresAt = expiresAt;
            Used = false;
        }

        public bool IsExpired() => DateTimeOffset.UtcNow > ExpiresAt;

        public void MarkUsed()
        {
            Used = true;
        }
    }
}