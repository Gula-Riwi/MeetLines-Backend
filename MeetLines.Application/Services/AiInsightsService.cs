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

            // Sequential execution to avoid DbContext concurrency issues
            
            // 1. Get Metrics Summary (Used for Revenue and BotPerformance)
            var metricsSummary = await _botMetricsRepo.GetSummaryAsync(projectId, thirtyDaysAgo, now, ct);

            // 2. Run Calculations
            var staffing = await CalculateStaffingRecommendationsAsync(projectId, thirtyDaysAgo, now, ct);
            var churnForRisk = await CalculateChurnRiskAsync(projectId, thirtyDaysAgo, now, ct);
            var revenue = await CalculateRevenueOpportunityAsync(projectId, thirtyDaysAgo, now, metricsSummary, ct);
            var goldenHour = await CalculateGoldenHourAsync(projectId, thirtyDaysAgo, now, ct);

            // 3. Map Bot Performance
            var botPerformance = new BotPerformanceDto
            {
                TotalConversations = metricsSummary.TotalConversations,
                BotConversations = metricsSummary.BotConversations,
                HumanConversations = metricsSummary.HumanConversations,
                AppointmentsBooked = metricsSummary.TotalAppointments,
                ConversionRate = metricsSummary.AverageConversionRate,
                AverageResponseTime = metricsSummary.AverageResponseTime,
                CustomerSatisfactionScore = metricsSummary.AverageCustomerSatisfaction
            };

            return new AiInsightsDto
            {
                Staffing = staffing,
                ChurnRisks = churnForRisk,
                Revenue = revenue,
                Optimization = goldenHour,
                BotPerformance = botPerformance
            };
        }

        private async Task<StaffingRecommendationDto> CalculateStaffingRecommendationsAsync(Guid projectId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
        {
            var appointments = await _appointmentRepo.GetByDateRangeAsync(projectId, start, end, ct);
            if (!appointments.Any()) return new StaffingRecommendationDto { Message = "No hay suficientes datos de citas." };

            // Logic: Find busiest hour block across all days
            var groupedByDayHour = appointments
                .GroupBy(a => new { a.StartTime.DayOfWeek, Hour = a.StartTime.Hour })
                .Select(g => new { g.Key.DayOfWeek, g.Key.Hour, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            if (groupedByDayHour != null && groupedByDayHour.Count > 3) // Threshold
            {
                var culture = new System.Globalization.CultureInfo("es-ES");
                var dayName = culture.DateTimeFormat.GetDayName(groupedByDayHour.DayOfWeek);
                dayName = char.ToUpper(dayName[0]) + dayName.Substring(1);

                return new StaffingRecommendationDto
                {
                    ActionRequired = true,
                    DayOfWeek = dayName,
                    TimeBlock = $"{groupedByDayHour.Hour}:00 - {groupedByDayHour.Hour + 1}:00",
                    ProjectedLostAppointments = (int)(groupedByDayHour.Count * 0.2), 
                    Message = $"Alta demanda los {dayName} a las {groupedByDayHour.Hour}:00. Refuerza tu equipo para no perder ventas."
                };
            }

            return new StaffingRecommendationDto { ActionRequired = false, Message = "Tu equipo cubre perfectamente la demanda actual." };
        }

        // ChurnRisk kept same...
        private async Task<List<ChurnRiskDto>> CalculateChurnRiskAsync(Guid projectId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
        {
            var feedbacks = await _feedbackRepo.GetByDateRangeAsync(projectId, start, end, ct);
            var risks = new List<ChurnRiskDto>();

            var highRisk = feedbacks.Where(f => f.Rating <= 2).ToList();
            if (highRisk.Any())
                risks.Add(new ChurnRiskDto { RiskLevel = "Alto", Count = highRisk.Count, AverageSentiment = highRisk.Average(f => f.Sentiment ?? 0) });

            var medRisk = feedbacks.Where(f => f.Rating == 3).ToList();
            if (medRisk.Any())
                risks.Add(new ChurnRiskDto { RiskLevel = "Medio", Count = medRisk.Count, AverageSentiment = medRisk.Average(f => f.Sentiment ?? 0) });

            return risks;
        }

        private async Task<RevenueOpportunityDto> CalculateRevenueOpportunityAsync(Guid projectId, DateTimeOffset start, DateTimeOffset end, MeetLines.Domain.Repositories.BotMetricsSummary metricsSummary, CancellationToken ct)
        {
            // 2. Calculate Unconverted
            var unconverted = Math.Max(0, metricsSummary.TotalConversations - metricsSummary.TotalAppointments);

            // 3. Calculate Average Ticket
            var totalSales = await _appointmentRepo.GetTotalSalesAsync(projectId, start, end, ct);
            var totalAppointments = metricsSummary.TotalAppointments; 
            
            if (totalAppointments == 0)
            {
                 var appts = await _appointmentRepo.GetByDateRangeAsync(projectId, start, end, ct);
                 totalAppointments = appts.Count();
            }

            if (totalAppointments <= 0) totalAppointments = 1;

            var avgTicket = totalSales / totalAppointments;
            if (avgTicket == 0) avgTicket = 20000; 

            var lostRevenue = unconverted * avgTicket * 0.1m; 

            // Generar sugerencia "Chimba" basada en datos
            string suggestion;
            if (unconverted > 20)
                suggestion = $"🚨 ¡Estás dejando perder plata! Tienes {unconverted} chats sin cerrar. Lanza una campaña de reactivación YA.";
            else if (metricsSummary.AverageConversionRate < 2.0 && metricsSummary.TotalConversations > 10)
                suggestion = "📉 Tu tasa de cierre está baja. Revisa cómo responde el bot y ajusta el 'Tone' en configuración.";
            else if (metricsSummary.HumanConversations > metricsSummary.BotConversations && metricsSummary.TotalConversations > 10)
                suggestion = "🤖 El bot está pasando demasiados clientes a humanos. Mejora tu Base de Conocimiento para automatizar más.";
            else if (lostRevenue > 500000)
                suggestion = $"💸 Podrías estar ganando ${lostRevenue:N0} extra. Contacta a esos {unconverted} clientes pasados.";
            else
                suggestion = "🚀 ¡Vas volando! Tu operación está optimizada. Sigue así.";

            return new RevenueOpportunityDto
            {
                TotalLostRevenue = lostRevenue,
                UnconvertedConversations = unconverted,
                AverageTicket = avgTicket,
                Suggestion = suggestion
            };
        }

        private async Task<GoldenHourDto> CalculateGoldenHourAsync(Guid projectId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
        {
            // Logic: When do customers write the most?
            var conversations = await _conversationRepo.GetByDateRangeAsync(projectId, start, end, ct);
            
            if (!conversations.Any()) return new GoldenHourDto { Suggestion = "Faltan datos de conversaciones." };

            var busiestSlot = conversations
                .GroupBy(c => c.CreatedAt.ToLocalTime().Hour) // Group by local hour (simplified) - ideal needs Project Timezone
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
