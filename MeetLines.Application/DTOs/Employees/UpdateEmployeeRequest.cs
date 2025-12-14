using System;

namespace MeetLines.Application.DTOs.Employees
{
    public class UpdateEmployeeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Area { get; set; }
    }
}
