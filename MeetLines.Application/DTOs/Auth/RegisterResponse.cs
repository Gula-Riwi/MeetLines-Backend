using System;

namespace MeetLines.Application.DTOs.Auth
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Plan { get; set; } = "beginner";
    }
}