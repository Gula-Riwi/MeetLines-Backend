using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IGeoIpService
    {
        Task<string?> GetTimezoneAsync(string ip, CancellationToken ct = default);
    }
}
