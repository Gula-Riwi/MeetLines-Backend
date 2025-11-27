using System;

namespace MeetLines.Domain.Entities
{
    public class LoginSession
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string TokenHash { get; private set; }
        public string? DeviceInfo { get; private set; }
        public string? IpAddress { get; private set; }
        public DateTimeOffset? ExpiresAt { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        // Constructor vacÃ­o para EF Core (si es necesario)
        private LoginSession() { }

        public LoginSession(Guid userId, string tokenHash, string? deviceInfo, string? ipAddress, DateTimeOffset? expiresAt)
        {
            if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("TokenHash cannot be empty", nameof(tokenHash));

            Id = Guid.NewGuid();
            UserId = userId;
            TokenHash = tokenHash;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            ExpiresAt = expiresAt;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
        }
    }
}
