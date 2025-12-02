using System;
using MeetLines.Domain.Enums;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Token para verificación de email después del registro
    /// </summary>
    public class EmailVerificationToken
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = null!;
        public DateTimeOffset ExpiresAt { get; private set; }
        public EmailVerificationStatus Status { get; private set; }
        public DateTimeOffset? VerifiedAt { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        // Constructor privado para EF Core
        private EmailVerificationToken() { }

        public EmailVerificationToken(Guid userId, string token, int expirationHours = 24)
        {
            if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token cannot be empty", nameof(token));

            Id = Guid.NewGuid();
            UserId = userId;
            Token = token;
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(expirationHours);
            Status = EmailVerificationStatus.Pending;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public bool IsExpired()
        {
            return DateTimeOffset.UtcNow >= ExpiresAt;
        }

        public bool CanBeUsed()
        {
            return Status == EmailVerificationStatus.Pending && !IsExpired();
        }

        public void MarkAsVerified()
        {
            if (!CanBeUsed())
                throw new InvalidOperationException("Token cannot be verified");

            Status = EmailVerificationStatus.Verified;
            VerifiedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsExpired()
        {
            Status = EmailVerificationStatus.Expired;
        }
    }
}