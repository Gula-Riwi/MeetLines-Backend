namespace MeetLines.Application.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Timezone { get; set; } = "UTC";
        // Optional: these will be auto-populated by the API when not provided
        public string? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }
    }
}