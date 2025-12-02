using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.Application.UseCases.HealthCheck;

public interface IHealthCheckUseCase
{
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}
