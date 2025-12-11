using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetLines.Infrastructure.Repositories
{
    public class ChannelRepository : IChannelRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public ChannelRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Channel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Channels
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<IEnumerable<Channel>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.Channels
                .Where(c => c.ProjectId == projectId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Channel> CreateAsync(Channel channel, CancellationToken ct = default)
        {
            await _context.Channels.AddAsync(channel, ct);
            await _context.SaveChangesAsync(ct);
            return channel;
        }

        public async Task UpdateAsync(Channel channel, CancellationToken ct = default)
        {
            _context.Channels.Update(channel);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Channel channel, CancellationToken ct = default)
        {
            _context.Channels.Remove(channel);
            await _context.SaveChangesAsync(ct);
        }
    }
}
