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
    /// EF Core implementation of IProjectBotConfigRepository (Adapter in Hexagonal Architecture)
    /// </summary>
    public class ProjectBotConfigRepository : IProjectBotConfigRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public ProjectBotConfigRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ProjectBotConfig?> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.ProjectBotConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProjectId == projectId, ct);
        }

        public async Task<ProjectBotConfig?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.ProjectBotConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<ProjectBotConfig> CreateAsync(ProjectBotConfig config, CancellationToken ct = default)
        {
            // Bypass tenant filtering when creating - validation happens at controller level
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _context.ProjectBotConfigs.Add(config);
            await _context.SaveChangesAsync(ct);
            return config;
        }

        public async Task UpdateAsync(ProjectBotConfig config, CancellationToken ct = default)
        {
            _context.ProjectBotConfigs.Update(config);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var config = await _context.ProjectBotConfigs.FindAsync(new object[] { id }, ct);
            if (config != null)
            {
                _context.ProjectBotConfigs.Remove(config);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<bool> ExistsForProjectAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.ProjectBotConfigs
                .AnyAsync(x => x.ProjectId == projectId, ct);
        }

        public async Task<IEnumerable<ProjectBotConfig>> GetByIndustryAsync(string industry, CancellationToken ct = default)
        {
            return await _context.ProjectBotConfigs
                .AsNoTracking()
                .Where(x => x.Industry == industry && x.IsActive)
                .ToListAsync(ct);
        }
    }
}
