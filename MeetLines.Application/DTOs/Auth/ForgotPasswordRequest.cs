namespace MeetLines.Application.DTOs.Auth
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }
}