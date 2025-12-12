using System.Text.Json.Serialization;

namespace MeetLines.Application.DTOs.Appointments
{
    public class ServicePublicDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "COP";

        [JsonPropertyName("durationMinutes")]
        public int DurationMinutes { get; set; }
    }
}
