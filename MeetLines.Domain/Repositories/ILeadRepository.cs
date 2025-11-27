using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface ILeadRepository
    {
        Task<Lead?> GetAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Lead>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
        Task AddAsync(Lead lead, CancellationToken ct = default);
        Task UpdateAsync(Lead lead, CancellationToken ct = default);
        Task AddInteractionAsync(LeadInteraction interaction, CancellationToken ct = default);
    }
}
