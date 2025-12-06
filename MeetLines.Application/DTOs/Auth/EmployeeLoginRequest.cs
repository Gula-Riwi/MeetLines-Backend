using System.ComponentModel.DataAnnotations;

namespace MeetLines.Application.DTOs.Auth
{
    public class EmployeeLoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
    }
}
