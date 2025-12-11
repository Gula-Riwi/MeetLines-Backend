using System;
using System.Text.Json.Serialization;

namespace MeetLines.Application.DTOs.Projects
{
    public class ProjectPublicSummaryDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("industry")]
        public string Industry { get; set; } = string.Empty;
    }
}
