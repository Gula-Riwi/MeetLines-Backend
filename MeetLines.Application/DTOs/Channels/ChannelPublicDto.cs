using System;
using System.Text.Json.Serialization;

namespace MeetLines.Application.DTOs.Channels
{
    public class ChannelPublicDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty; // The JSON string containing the link/handle
    }
}
