using System;

namespace MeetLines.Domain.Entities
{
    public class AppUser
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; } = null!;
        public string? PasswordHash { get; private set; }
        public string FullName { get; private set; } = null!;
        public string? Phone { get; private set; }
        public bool IsEmailVerified { get; private set; }
        public bool IsPhoneVerified { get; private set; }
        public string AuthProvider { get; private set; } = "email";
        public string? ExternalProviderId { get; private set; }
        
        // Two-Factor Authentication
        public bool TwoFactorEnabled { get; private set; }
        public string? TwoFactorSecret { get; private set; }
        
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private AppUser() { } // EF Core

        public AppUser(string email, string fullName, string? phone = null, string authProvider = "bot")
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName cannot be empty", nameof(fullName));

            Id = Guid.NewGuid();
            Email = email.ToLowerInvariant();
            FullName = fullName;
            Phone = phone;
            IsEmailVerified = false;
            IsPhoneVerified = false;
            AuthProvider = authProvider;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateInfo(string fullName, string? phone)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName cannot be empty", nameof(fullName));
            
            FullName = fullName;
            Phone = phone;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void VerifyEmail()
        {
            IsEmailVerified = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void EnableTwoFactor(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("Two-factor secret cannot be empty", nameof(secret));
            
            TwoFactorSecret = secret;
            TwoFactorEnabled = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void DisableTwoFactor()
        {
            TwoFactorEnabled = false;
            TwoFactorSecret = null;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
