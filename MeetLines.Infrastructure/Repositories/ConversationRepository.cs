using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;

namespace MeetLines.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of IConversationRepository (Adapter in Hexagonal Architecture)
    /// </summary>
    public class ConversationRepository : IConversationRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public ConversationRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Conversation?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Conversations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IEnumerable<Conversation>> GetByProjectIdAsync(Guid projectId, int skip, int take, CancellationToken ct = default)
        {
            return await _context.Conversations
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Conversation>> GetByCustomerPhoneAsync(Guid projectId, string customerPhone, CancellationToken ct = default)
        {
            return await _context.Conversations
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.CustomerPhone == customerPhone)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Conversation>> GetByBotTypeAsync(Guid projectId, string botType, CancellationToken ct = default)
        {
            return await _context.Conversations
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.BotType == botType)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Conversation>> GetRequiringHumanAttentionAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.Conversations
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.RequiresHumanAttention && !x.HandledByHuman)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Conversation>> GetByDateRangeAsync(Guid projectId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
        {
            return await _context.Conversations
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Conversation> CreateAsync(Conversation conversation, CancellationToken ct = default)
        {
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync(ct);
            return conversation;
        }

        public async Task UpdateAsync(Conversation conversation, CancellationToken ct = default)
        {
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync(ct);
        }

        public async Task MarkAsHandledByHumanAsync(Guid id, Guid employeeId, CancellationToken ct = default)
        {
            var conversation = await _context.Conversations.FindAsync(new object[] { id }, ct);
            if (conversation != null)
            {
                conversation.AssignToEmployee(employeeId);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<int> GetCountByProjectAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.Conversations
                .Where(x => x.ProjectId == projectId)
                .CountAsync(ct);
        }

        public async Task<double?> GetAverageSentimentAsync(Guid projectId, DateTime? startDate = null, CancellationToken ct = default)
        {
            var query = _context.Conversations
                .Where(x => x.ProjectId == projectId && x.Sentiment.HasValue);

            if (startDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= startDate.Value);
            }

            return await query.AverageAsync(x => x.Sentiment, ct);
        }
    }
}
