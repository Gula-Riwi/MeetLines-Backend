using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.AiInsights;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class AiInsightsService : IAiInsightsService
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly ICustomerFeedbackRepository _feedbackRepo;
        private readonly IConversationRepository _conversationRepo;
        private readonly IBotMetricsRepository _botMetricsRepo;

        public AiInsightsService(
            IAppointmentRepository appointmentRepo,
            ICustomerFeedbackRepository feedbackRepo,
            IConversationRepository conversationRepo,
            IBotMetricsRepository botMetricsRepo)
        {
            _appointmentRepo = appointmentRepo;
            _feedbackRepo = feedbackRepo;
            _conversationRepo = conversationRepo;
            _botMetricsRepo = botMetricsRepo;
        }

        public async Task<AiInsightsDto> GetProjectInsightsAsync(Guid projectId, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);

            // Parallel execution for performance
            var staffingTask = CalculateStaffingRecommendationsAsync(projectId, thirtyDaysAgo, now, ct);
            var churnTask = CalculateChurnRiskAsync(projectId, thirtyDaysAgo, now, ct);
            var revenueTask = CalculateRevenueOpportunityAsync(projectId, thirtyDaysAgo, now, ct);
            var goldenHourTask = CalculateGoldenHourAsync(projectId, thirtyDaysAgo, now, ct);

            await Task.WhenAll(staffingTask, churnTask, revenueTask, goldenHourTask);

            return new AiInsightsDto
            {
                Staffing = await staffingTask,
                ChurnRisks = await churnTask,
                Revenue = await revenueTask,
                Optimization = await goldenHourTask
            };
        }

        private async Task<StaffingRecommendationDto> CalculateStaffingRecommendationsAsync(Guid projectId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
        {
            var appointments = await _appointmentRepo.GetByDateRangeAsync(projectId, start, end, ct);
            if (!appointments.Any()) return new StaffingRecommendationDto { Message = "No hay suficientes datos de citas." };

            // Logic: Find busiest hour block across all days
            // Fix: Convert to Project Timezone (-05:00) BEFORE grouping
            var projectOffset = TimeSpan.FromHours(-5);
            
            var groupedByDayHour = appointments
                .Select(a => a.StartTime.ToOffset(projectOffset))
                .GroupBy(dt => new { dt.DayOfWeek, dt.Hour })
                .Select(g => new { g.Key.DayOfWeek, g.Key.Hour, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            if (groupedByDayHour != null && groupedByDayHour.Count > 3) // Threshold: > 3 appointments starting in same hour block is "busy"
            {
                 // Spanish day translation
                var culture = new System.Globalization.CultureInfo("es-ES");
                var dayName = culture.DateTimeFormat.GetDayName(groupedByDayHour.DayOfWeek);

                return new StaffingRecommendationDto
                {
                    ActionRequired = true,
                    DayOfWeek = dayName,
                    TimeBlock = $"{groupedByDayHour.Hour}:00 - {groupedByDayHour.Hour + 1}:00",
                    ProjectedLostAppointments = (int)(groupedByDayHour.Count * 0.2), // Simple heuristic: 20% overflow
                    Message = $"Alta demanda detectada los {dayName} a las {groupedByDayHour.Hour}:00. Recomendamos refuerzo de personal."
                };
            }

            return new StaffingRecommendationDto { ActionRequired = false, Message = "Carga de trabajo equilibrada." };
        }

        private async Task<List<ChurnRiskDto>> CalculateChurnRiskAsync(Guid projectId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
        {
            // Logic: Analyze feedback ratings from last 30 days
            var feedbacks = await _feedbackRepo.GetByDateRangeAsync(projectId, start, end, ct);
            
            var risks = new List<ChurnRiskDto>();

            // Segment: High Risk (Rating <= 2)
            var highRisk = feedbacks.Where(f => f.Rating <= 2).ToList();
            risks.Add(new ChurnRiskDto 
            { 
                RiskLevel = "Alto", 
                Count = highRisk.Count, 
                AverageSentiment = highRisk.Any() ? highRisk.Average(f => f.Sentiment ?? 0) : 0 
            });

             // Segment: Medium Risk (Rating == 3)
            var medRisk = feedbacks.Where(f => f.Rating == 3).ToList();
            risks.Add(new ChurnRiskDto 
            { 
                RiskLevel = "Medio", 
                Count = medRisk.Count, 
                AverageSentiment = medRisk.Any() ? medRisk.Average(f => f.Sentiment ?? 0) : 0 
            });

            return risks;
        }

        private async Task<RevenueOpportunityDto> CalculateRevenueOpportunityAsync(Guid projectId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
        {
            // 1. Get Conversation Metrics
            var metricsSummary = await _botMetricsRepo.GetSummaryAsync(projectId, start, end, ct);
            
            // 2. Calculate Unconverted (Total - Booked)
            // Note: BotMetricsSummary might not be fully populated if jobs haven't run, so fallback logic is needed.
            // Or use raw counts if summary returns 0. For now rely on summary.
            var unconverted = Math.Max(0, metricsSummary.TotalConversations - metricsSummary.TotalAppointments);

            // 3. Calculate Average Ticket
            var totalSales = await _appointmentRepo.GetTotalSalesAsync(projectId, start, end, ct);
            var totalAppointments = metricsSummary.TotalAppointments > 0 ? metricsSummary.TotalAppointments : 1; 
            
            // Fallback: If no metrics, try counting appointments directly
            if (metricsSummary.TotalAppointments == 0)
            {
                 var appts = await _appointmentRepo.GetByDateRangeAsync(projectId, start, end, ct);
                 totalAppointments = appts.Count() > 0 ? appts.Count() : 1;
            }

            var avgTicket = totalSales / totalAppointments;
            if (avgTicket == 0) avgTicket = 20000; // Default fallback (e.g. 20k COP)

            var lostRevenue = unconverted * avgTicket * 0.1m; // Assumption: we could close 10% of unconverted

            return new RevenueOpportunityDto
            {
                TotalLostRevenue = lostRevenue,
                UnconvertedConversations = unconverted,
                AverageTicket = avgTicket,
                Suggestion = unconverted > 10 
                    ? $"Tienes {unconverted} chats sin cerrar. Podrías generar ${lostRevenue:N0} extra activando descuentos de reactivación."
                    : "Tasa de conversión saludable."
            };
        }

        private async Task<GoldenHourDto> CalculateGoldenHourAsync(Guid projectId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
        {
            // Logic: When do customers write the most?
            var conversations = await _conversationRepo.GetByDateRangeAsync(projectId, start, end, ct);
            
            if (!conversations.Any()) return new GoldenHourDto { Suggestion = "Faltan datos de conversaciones." };

            // Fix: Use fixed offset for Colombia (-05:00) instead of Server Local Time
            var projectOffset = TimeSpan.FromHours(-5);

            var busiestSlot = conversations
                .GroupBy(c => c.CreatedAt.ToOffset(projectOffset).Hour) 
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            if (busiestSlot != null)
            {
                 return new GoldenHourDto
                 {
                     BestDay = "Todos", // Aggregate
                     BestTime = $"{busiestSlot.Hour}:00",
                     ResponseRate = 0, // Not easily calcable without message logs, placeholder
                     Suggestion = $"Tus clientes están más activos a las {busiestSlot.Hour}:00. Programa tus campañas a esa hora."
                 };
            }

            return new GoldenHourDto();
        }
    }
}
