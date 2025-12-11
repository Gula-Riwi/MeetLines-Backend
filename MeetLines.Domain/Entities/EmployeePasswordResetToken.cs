using System;
using MeetLines.Domain.Enums;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Token para recuperación de contraseña de empleados
    /// </summary>
    public class EmployeePasswordResetToken
    {
        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }
        public string Token { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }
        public PasswordResetTokenStatus Status { get; private set; }
        public DateTimeOffset? UsedAt { get; private set; }
        public string? IpAddress { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        // Constructor privado para EF Core
        private EmployeePasswordResetToken() { Token = null!; }

        public EmployeePasswordResetToken(Guid employeeId, string token, int expirationHours = 1, string? ipAddress = null)
        {
            if (employeeId == Guid.Empty) throw new ArgumentException("EmployeeId cannot be empty", nameof(employeeId));
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token cannot be empty", nameof(token));

            Id = Guid.NewGuid();
            EmployeeId = employeeId;
            Token = token;
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(expirationHours);
            Status = PasswordResetTokenStatus.Active;
            IpAddress = ipAddress;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public bool IsExpired()
        {
            return DateTimeOffset.UtcNow >= ExpiresAt;
        }

        public bool CanBeUsed()
        {
            return Status == PasswordResetTokenStatus.Active && !IsExpired();
        }

        public void MarkAsUsed()
        {
            if (!CanBeUsed())
                throw new InvalidOperationException("Token cannot be used");

            Status = PasswordResetTokenStatus.Used;
            UsedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsExpired()
        {
            Status = PasswordResetTokenStatus.Expired;
        }
    }
}
