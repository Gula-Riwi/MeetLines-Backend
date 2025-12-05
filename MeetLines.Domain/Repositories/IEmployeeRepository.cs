using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Employee?> GetByUsernameAsync(string username, CancellationToken ct = default);
        Task<IEnumerable<Employee>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
        Task<IEnumerable<Employee>> GetByAreaAsync(Guid projectId, string area, CancellationToken ct = default);
        Task AddAsync(Employee employee, CancellationToken ct = default);
        Task UpdateAsync(Employee employee, CancellationToken ct = default);
    }
}
