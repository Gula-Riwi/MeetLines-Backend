using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IChannelRepository
    {
        Task<Channel?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Channel>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
        Task<Channel> CreateAsync(Channel channel, CancellationToken ct = default);
        Task UpdateAsync(Channel channel, CancellationToken ct = default);
        Task DeleteAsync(Channel channel, CancellationToken ct = default);
    }
}
