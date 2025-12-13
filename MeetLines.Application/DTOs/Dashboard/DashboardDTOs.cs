using System;

namespace MeetLines.Application.DTOs.Dashboard
{
    public class DashboardStatsResponse
    {
        public DashboardMetric MonthlySales { get; set; } = new();
        public DashboardMetric AiConversations { get; set; } = new();
        public DashboardMetric AutomatedOrders { get; set; } = new();
        public DashboardMetric HoursSaved { get; set; } = new();
    }

    public class DashboardMetric
    {
        public double Value { get; set; }
        public double PercentageChange { get; set; } 
        public string Trend { get; set; } = "neutral"; // "up", "down", "neutral"
    }

    public class DashboardTaskDto
    {
        public string Id { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTimeOffset Date { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Currency { get; set; } = "COP";
    }
}
