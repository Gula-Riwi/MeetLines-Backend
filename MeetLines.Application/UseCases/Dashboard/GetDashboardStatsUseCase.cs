using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Dashboard;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Dashboard
{
    public class GetDashboardStatsUseCase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IBotMetricsRepository _botMetricsRepository;

        public GetDashboardStatsUseCase(
            IAppointmentRepository appointmentRepository,
            IBotMetricsRepository botMetricsRepository)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _botMetricsRepository = botMetricsRepository ?? throw new ArgumentNullException(nameof(botMetricsRepository));
        }

        public async Task<Result<DashboardStatsResponse>> ExecuteAsync(Guid projectId, CancellationToken ct = default)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var currentMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                var currentMonthEnd = currentMonthStart.AddMonths(1).AddTicks(-1);
                
                var prevMonthStart = currentMonthStart.AddMonths(-1);
                var prevMonthEnd = currentMonthStart.AddTicks(-1);

                // 1. Get Sales Stats
                var currentSales = await _appointmentRepository.GetTotalSalesAsync(projectId, currentMonthStart, currentMonthEnd, ct);
                var prevSales = await _appointmentRepository.GetTotalSalesAsync(projectId, prevMonthStart, prevMonthEnd, ct);

                // 2. Get Bot Metrics
                var currentMetrics = await _botMetricsRepository.GetSummaryAsync(projectId, currentMonthStart, currentMonthEnd, ct);
                var prevMetrics = await _botMetricsRepository.GetSummaryAsync(projectId, prevMonthStart, prevMonthEnd, ct);

                // 3. Calculate metrics
                var response = new DashboardStatsResponse
                {
                    MonthlySales = CalculateMetric(currentSales, prevSales),
                    AiConversations = CalculateMetric((double)currentMetrics.TotalConversations, (double)prevMetrics.TotalConversations),
                    AutomatedOrders = CalculateMetric((double)currentMetrics.TotalAppointments, (double)prevMetrics.TotalAppointments),
                    HoursSaved = CalculateHoursSaved(currentMetrics.TotalConversations, currentMetrics.TotalAppointments, 
                                                     prevMetrics.TotalConversations, prevMetrics.TotalAppointments)
                };

                return Result<DashboardStatsResponse>.Ok(response);
            }
            catch (Exception ex)
            {
                return Result<DashboardStatsResponse>.Fail(ex.Message);
            }
        }

        private DashboardMetric CalculateMetric(double current, double prev)
        {
            var diff = current - prev;
            var percentage = prev != 0 ? (diff / prev) * 100 : (current > 0 ? 100 : 0);
            
            return new DashboardMetric
            {
                Value = Math.Round(current, 2),
                PercentageChange = Math.Round(percentage, 1),
                Trend = percentage > 0 ? "up" : (percentage < 0 ? "down" : "neutral")
            };
        }
        
        // Overload for decimal (Money)
        private DashboardMetric CalculateMetric(decimal current, decimal prev)
        {
            return CalculateMetric((double)current, (double)prev);
        }

        private DashboardMetric CalculateHoursSaved(int currConvos, int currAppts, int prevConvos, int prevAppts)
        {
            // Formula: (Convos * 5min) + (Appts * 15min)
            double currentHours = ((currConvos * 5) + (currAppts * 15)) / 60.0;
            double prevHours = ((prevConvos * 5) + (prevAppts * 15)) / 60.0;

            return CalculateMetric(currentHours, prevHours);
        }
    }
}
