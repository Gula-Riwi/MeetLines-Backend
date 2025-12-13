using System;

namespace MeetLines.Domain.Entities
{
    public class AppUserPasswordResetToken
    {
        public Guid Id { get; private set; }
        public Guid AppUserId { get; private set; }
        public string Token { get; private set; } = null!;
        public DateTimeOffset ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private AppUserPasswordResetToken() { } // EF Core

        public AppUserPasswordResetToken(Guid appUserId, string token, int expiryHours = 24)
        {
            if (appUserId == Guid.Empty) throw new ArgumentException("AppUserId cannot be empty", nameof(appUserId));
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token cannot be empty", nameof(token));

            Id = Guid.NewGuid();
            AppUserId = appUserId;
            Token = token;
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(expiryHours);
            IsUsed = false;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public bool CanBeUsed()
        {
            return !IsUsed && ExpiresAt > DateTimeOffset.UtcNow;
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
        }
    }
}
