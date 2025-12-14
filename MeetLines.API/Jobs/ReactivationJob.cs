using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using MeetLines.Application.Services.Interfaces;

namespace MeetLines.API.Jobs
{
    public class ReactivationJob
    {
        private readonly ICustomerReactivationService _reactivationService;
        private readonly ILogger<ReactivationJob> _logger;

        public ReactivationJob(
            ICustomerReactivationService reactivationService,
            ILogger<ReactivationJob> logger)
        {
            _reactivationService = reactivationService ?? throw new ArgumentNullException(nameof(reactivationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Hangfire Job: executing Customer Reactivation...");
            await _reactivationService.ProcessDailyReactivationsAsync();
            _logger.LogInformation("Hangfire Job: Customer Reactivation completed.");
        }
    }
}
