using System;

namespace MeetLines.Domain.Entities
{
    public class Employee
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Name { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; } // New Property
        public string Phone { get; private set; } // New Property for WhatsApp Alerts
        public string PasswordHash { get; private set; }
        public string Role { get; private set; }
        public string Area { get; private set; } // New Property
        public bool IsActive { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        // Constructor for EF Core
        private Employee() 
        { 
            Name = null!; 
            Username = null!; 
            Email = null!;
            Phone = null!;
            PasswordHash = null!; 
            Role = null!; 
            Area = null!;
        }



        public Employee(Guid projectId, string name, string username, string email, string passwordHash, string role = "Employee", string area = "General")
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty", nameof(username));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("PasswordHash cannot be empty", nameof(passwordHash));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            Name = name;
            Username = username.ToLowerInvariant(); // Normalize username
            Email = email;
            Phone = ""; // Default empty
            PasswordHash = passwordHash;
            Role = role;
            Area = area;
            IsActive = true;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateDetails(string name, string role, string area, string? phone = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            
            Name = name;
            Role = role;
            Area = area;
            if (phone != null) Phone = phone;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("PasswordHash cannot be empty", nameof(newPasswordHash));
            
            PasswordHash = newPasswordHash;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
