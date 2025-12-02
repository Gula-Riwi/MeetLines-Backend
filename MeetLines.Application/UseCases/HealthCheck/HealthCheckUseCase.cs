using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.Application.UseCases.HealthCheck;

public class HealthCheckUseCase : IHealthCheckUseCase
{
    public Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        // Simple health check logic. Could be expanded to check DB, external services, etc.
        return Task.FromResult(true);
    }
}
