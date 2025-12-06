using System;
using System.ComponentModel.DataAnnotations;

namespace MeetLines.Application.DTOs.Employees
{
    public class CreateEmployeeRequest
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Username is now auto-generated from email
        public string? Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;



        public string Role { get; set; } = "Employee";
        
        public string Area { get; set; } = "General";
    }
}
