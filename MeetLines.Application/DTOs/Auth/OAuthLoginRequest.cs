using MeetLines.Domain.Enums;

namespace MeetLines.Application.DTOs.Auth
{
    public class OAuthLoginRequest
    {
        public string ExternalProviderId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public AuthProvider Provider { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
    }
}