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
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        
        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }
        
        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }
        
        // Calculated distance in KM if coordinates provided
        [JsonPropertyName("distanceKm")]
        public double? DistanceKm { get; set; }
    }
}
