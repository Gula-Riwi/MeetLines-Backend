using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.Application.Services.Interfaces
{
    public interface ICustomerReactivationService
    {
        /// <summary>
        /// Daily job to process reactivation for all projects
        /// </summary>
        Task ProcessDailyReactivationsAsync(CancellationToken ct = default);
    }
}
