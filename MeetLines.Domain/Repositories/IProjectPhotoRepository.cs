using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IProjectPhotoRepository
    {
        Task<ProjectPhoto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<ProjectPhoto>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
        Task<int> CountByProjectIdAsync(Guid projectId, CancellationToken ct = default);
        Task AddAsync(ProjectPhoto photo, CancellationToken ct = default);
        Task DeleteAsync(ProjectPhoto photo, CancellationToken ct = default);
        Task<ProjectPhoto?> GetMainPhotoAsync(Guid projectId, CancellationToken ct = default);
    }
}
