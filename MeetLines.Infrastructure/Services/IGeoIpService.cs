using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.Infrastructure.Services
{
    public interface IGeoIpService
    {
        /// <summary>
        /// Devuelve la zona horaria asociada a la IP, o null si no se puede determinar
        /// </summary>
        Task<string?> GetTimezoneAsync(string ip, CancellationToken ct = default);
    }
}
