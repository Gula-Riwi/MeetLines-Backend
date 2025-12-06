using System;

namespace MeetLines.Application.DTOs.Employees
{
    public class EmployeeResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
