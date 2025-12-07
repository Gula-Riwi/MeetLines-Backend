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
    /// EF Core implementation of IKnowledgeBaseRepository (Adapter in Hexagonal Architecture)
    /// </summary>
    public class KnowledgeBaseRepository : IKnowledgeBaseRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public KnowledgeBaseRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<KnowledgeBase?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.KnowledgeBases
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IEnumerable<KnowledgeBase>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.KnowledgeBases
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.Category)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<KnowledgeBase>> GetActiveByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.KnowledgeBases
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.IsActive)
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.Category)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<KnowledgeBase>> GetByCategoryAsync(Guid projectId, string category, CancellationToken ct = default)
        {
            return await _context.KnowledgeBases
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.Category == category && x.IsActive)
                .OrderByDescending(x => x.Priority)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<KnowledgeBase>> SearchAsync(Guid projectId, string query, CancellationToken ct = default)
        {
            // Full-text search - search in question and answer only
            // Keywords search is done in-memory after fetching results
            var lowerQuery = query.ToLower();
            
            var results = await _context.KnowledgeBases
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.IsActive)
                .Where(x => 
                    EF.Functions.ILike(x.Question, $"%{lowerQuery}%") ||
                    EF.Functions.ILike(x.Answer, $"%{lowerQuery}%"))
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.UsageCount)
                .ToListAsync(ct);
            
            // Also search in keywords (in-memory since JSONB is complex)
            var keywordMatches = await _context.KnowledgeBases
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.IsActive)
                .ToListAsync(ct);
            
            keywordMatches = keywordMatches
                .Where(x => x.Keywords != null && x.Keywords.ToLower().Contains(lowerQuery))
                .ToList();
            
            // Combine and deduplicate
            return results.Union(keywordMatches)
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.UsageCount)
                .ToList();
        }

        public async Task<KnowledgeBase> CreateAsync(KnowledgeBase knowledgeBase, CancellationToken ct = default)
        {
            _context.KnowledgeBases.Add(knowledgeBase);
            await _context.SaveChangesAsync(ct);
            return knowledgeBase;
        }

        public async Task UpdateAsync(KnowledgeBase knowledgeBase, CancellationToken ct = default)
        {
            _context.KnowledgeBases.Update(knowledgeBase);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var kb = await _context.KnowledgeBases.FindAsync(new object[] { id }, ct);
            if (kb != null)
            {
                _context.KnowledgeBases.Remove(kb);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task IncrementUsageAsync(Guid id, CancellationToken ct = default)
        {
            var kb = await _context.KnowledgeBases.FindAsync(new object[] { id }, ct);
            if (kb != null)
            {
                kb.UsageCount++;
                kb.LastUsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<IEnumerable<KnowledgeBase>> GetMostUsedAsync(Guid projectId, int count, CancellationToken ct = default)
        {
            return await _context.KnowledgeBases
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && x.IsActive)
                .OrderByDescending(x => x.UsageCount)
                .Take(count)
                .ToListAsync(ct);
        }
    }
}
