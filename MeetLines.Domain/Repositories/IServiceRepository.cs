using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IServiceRepository
    {
        Task<Service?> GetAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<Service>> GetByProjectIdAsync(Guid projectId, bool activeOnly = true, CancellationToken ct = default);
        Task<Service> CreateAsync(Service service, CancellationToken ct = default);
        Task UpdateAsync(Service service, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
