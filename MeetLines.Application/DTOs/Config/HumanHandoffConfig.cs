using System.Collections.Generic;

namespace MeetLines.Application.DTOs.Config
{
    public class HumanHandoffConfig
    {
        public bool TestMode { get; set; } = false;
        public bool MultiAgent { get; set; } = false;
        public bool HumanFallback { get; set; } = true;
        public string? TestPhoneNumber { get; set; }
        public string HumanFallbackMessage { get; set; } = "Te conecto con un miembro de nuestro equipo.";
        public string HumanFallbackKeywords { get; set; } = "hablar con persona,hablar con humano,operador";
        public string AgentAssignmentStrategy { get; set; } = "round-robin"; // "round-robin", "random"
        public List<string>? TeamNotificationNumbers { get; set; }
    }
}
