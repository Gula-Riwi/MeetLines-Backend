using System;

namespace MeetLines.Application.DTOs.BotSystem
{
    /// <summary>
    /// DTO for bot metrics
    /// </summary>
    public class BotMetricsDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public DateTime Date { get; set; }
        public int TotalConversations { get; set; }
        public int BotConversations { get; set; }
        public int HumanConversations { get; set; }
        public int AppointmentsBooked { get; set; }
        public double ConversionRate { get; set; }
        public double? AverageFeedbackRating { get; set; }
        public int CustomersReactivated { get; set; }
        public double ReactivationRate { get; set; }
        public double AverageResponseTime { get; set; }
        public double CustomerSatisfactionScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for metrics summary
    /// </summary>
    public class BotMetricsSummaryDto
    {
        public int TotalConversations { get; set; }
        public int TotalAppointments { get; set; }
        public double AverageConversionRate { get; set; }
        public double AverageFeedbackRating { get; set; }
        public int TotalReactivations { get; set; }
        public double AverageReactivationRate { get; set; }
        public double AverageResponseTime { get; set; }
        public double AverageCustomerSatisfaction { get; set; }
    }

    /// <summary>
    /// DTO for metrics query
    /// </summary>
    public class GetMetricsRequest
    {
        public Guid ProjectId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? LastNDays { get; set; }
    }
}
