using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using MeetLines.Application.Services.Interfaces;

namespace MeetLines.API.Jobs
{
    public class DailyMetricsJob
    {
        private readonly IBotMetricsService _botMetricsService;
        private readonly ILogger<DailyMetricsJob> _logger;

        public DailyMetricsJob(
            IBotMetricsService botMetricsService,
            ILogger<DailyMetricsJob> logger)
        {
            _botMetricsService = botMetricsService ?? throw new ArgumentNullException(nameof(botMetricsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [DisableConcurrentExecution(timeoutInSeconds: 600)] // 10 minutes timeout
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Hangfire Job: executing Daily Bot Metrics Snapshot...");
            try
            {
                await _botMetricsService.ProcessDailyMetricsForAllProjectsAsync();
                _logger.LogInformation("Hangfire Job: Daily Bot Metrics Snapshot completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hangfire Job: Daily Bot Metrics Snapshot FAILED.");
                throw; // Rethrow to let Hangfire retry
            }
        }
    }
}
