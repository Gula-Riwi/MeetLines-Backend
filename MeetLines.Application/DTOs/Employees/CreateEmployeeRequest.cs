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

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "Employee";
        
        public string Area { get; set; } = "General";
    }
}
