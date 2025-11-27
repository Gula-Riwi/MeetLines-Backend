using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IProjectRepository
    {
        Task<Project?> GetAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Project>> GetByUserAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(Project project, CancellationToken ct = default);
        Task UpdateAsync(Project project, CancellationToken ct = default);
    }
}
