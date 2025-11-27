using System;

namespace MeetLines.Domain.Entities
{
    public class SaasUser
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string? Phone { get; private set; }
        public string Timezone { get; private set; }
        public string Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private SaasUser() { } // EF Core

        public SaasUser(string name, string email, string passwordHash, string timezone = "UTC")
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("PasswordHash cannot be empty", nameof(passwordHash));

            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Timezone = timezone;
            Status = "active";
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateProfile(string name, string? phone, string timezone)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            Name = name;
            Phone = phone;
            Timezone = timezone;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void ChangePassword(string newPasswordHash)
        {
             if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("PasswordHash cannot be empty", nameof(newPasswordHash));
             PasswordHash = newPasswordHash;
             UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
