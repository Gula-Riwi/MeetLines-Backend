using System;
using System.Text.Json.Serialization;

namespace MeetLines.Application.DTOs.Channels
{
    public class ChannelDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("projectId")]
        public Guid ProjectId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("credentials")]
        public string? Credentials { get; set; }
    }

    public class CreateChannelRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("credentials")]
        public string? Credentials { get; set; } // JSON string
    }

    public class UpdateChannelRequest
    {
        [JsonPropertyName("credentials")]
        public string? Credentials { get; set; } // JSON string
    }
}
