namespace MeetLines.Application.DTOs.Auth
{
    public class DiscordExchangeRequest
    {
        public string Code { get; set; } = string.Empty;
        public string? RedirectUri { get; set; }
    }
}
