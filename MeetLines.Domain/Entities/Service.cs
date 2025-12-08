using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Services offered by the business (haircut, massage, consultation, etc.)
    /// </summary>
    public class Service
    {
        public int Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public decimal Price { get; private set; }
        public string Currency { get; private set; }
        public int DurationMinutes { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // EF Core constructor
        private Service()
        {
            Name = null!;
            Currency = null!;
        }

        public Service(
            Guid projectId,
            string name,
            int durationMinutes,
            decimal price = 0,
            string currency = "COP",
            string? description = null)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            if (durationMinutes <= 0) throw new ArgumentException("Duration must be positive", nameof(durationMinutes));

            ProjectId = projectId;
            Name = name;
            Description = description;
            Price = price;
            Currency = currency;
            DurationMinutes = durationMinutes;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(string name, string? description, decimal price, int durationMinutes)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            if (durationMinutes <= 0) throw new ArgumentException("Duration must be positive", nameof(durationMinutes));

            Name = name;
            Description = description;
            Price = price;
            DurationMinutes = durationMinutes;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
