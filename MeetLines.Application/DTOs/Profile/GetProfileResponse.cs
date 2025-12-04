using System;
using MeetLines.Domain.Enums;

namespace MeetLines.Application.DTOs.Profile
{
    public class GetProfileResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Timezone { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public bool IsEmailVerified { get; set; }
        public AuthProvider AuthProvider { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}