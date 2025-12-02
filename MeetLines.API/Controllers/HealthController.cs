using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.UseCases.HealthCheck;
using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IHealthCheckUseCase _healthCheckUseCase;

        public HealthController(IHealthCheckUseCase healthCheckUseCase)
        {
            _healthCheckUseCase = healthCheckUseCase ?? throw new ArgumentNullException(nameof(healthCheckUseCase));
        }

        /// <summary>
        /// Simple health check endpoint.
        /// Returns 200 OK with "Healthy" when the service is running.
        /// </summary>
        [HttpGet]
        [Route("health")]
        public async Task<IActionResult> GetHealth(CancellationToken ct)
        {
            var healthy = await _healthCheckUseCase.IsHealthyAsync(ct);
            if (healthy)
                return Ok(new { status = "Healthy" });
            return StatusCode(503, new { status = "Unhealthy" });
        }
    }
}
