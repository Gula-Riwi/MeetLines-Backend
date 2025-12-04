namespace MeetLines.Application.DTOs.Profile
{
    public class UpdateProfileRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Timezone { get; set; } = string.Empty;
    }
}