using System;
using System.ComponentModel.DataAnnotations;

namespace MeetLines.Application.DTOs.Services
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateServiceRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        [Required]
        public string Currency { get; set; } = "COP";

        [Required]
        [Range(1, 1440, ErrorMessage = "Duration must be between 1 minute and 24 hours")]
        public int DurationMinutes { get; set; }
    }

    public class UpdateServiceRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? Currency { get; set; } // Optional update

        [Required]
        [Range(1, 1440)]
        public int DurationMinutes { get; set; }

        public bool? IsActive { get; set; }
    }
}
