using System;
using System.Collections.Generic;

namespace MeetLines.Application.DTOs.AiInsights
{
    public class AiInsightsDto
    {
        public StaffingRecommendationDto Staffing { get; set; } = new();
        public List<ChurnRiskDto> ChurnRisks { get; set; } = new();
        public RevenueOpportunityDto Revenue { get; set; } = new();
        public GoldenHourDto Optimization { get; set; } = new();
        public BotPerformanceDto BotPerformance { get; set; } = new();
    }

    public class StaffingRecommendationDto
    {
        public bool ActionRequired { get; set; }
        public string Message { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string TimeBlock { get; set; } = string.Empty; // e.g., "Afternoon"
        public int ProjectedLostAppointments { get; set; }
    }

    public class ChurnRiskDto
    {
        public string RiskLevel { get; set; } = string.Empty; // "High", "Medium", "Low"
        public int Count { get; set; }
        public double AverageSentiment { get; set; }
    }

    public class RevenueOpportunityDto
    {
        public decimal TotalLostRevenue { get; set; }
        public int UnconvertedConversations { get; set; }
        public decimal AverageTicket { get; set; } // Based on service prices
        public string Suggestion { get; set; } = string.Empty;
    }

    public class GoldenHourDto
    {
        public string BestDay { get; set; } = string.Empty;
        public string BestTime { get; set; } = string.Empty; // "19:00"
        public double ResponseRate { get; set; }
        public string Suggestion { get; set; } = string.Empty;
    }

    public class BotPerformanceDto
    {
        public int TotalConversations { get; set; }
        public int BotConversations { get; set; }
        public int HumanConversations { get; set; }
        public int AppointmentsBooked { get; set; }
        public double ConversionRate { get; set; }
        public double AverageResponseTime { get; set; }
        public double CustomerSatisfactionScore { get; set; }
    }
}
