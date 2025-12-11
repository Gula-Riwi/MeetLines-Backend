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
    /// Implementación del repositorio de proyectos
    /// </summary>
    public class ProjectRepository : IProjectRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public ProjectRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Project?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<IEnumerable<Project>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Projects
                .Where(p => p.UserId == userId && p.Status == "active")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Projects
                .Where(p => p.Status == "active")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<int> GetActiveCountByUserAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Projects
                .CountAsync(p => p.UserId == userId && p.Status == "active", ct);
        }

        public async Task AddAsync(Project project, CancellationToken ct = default)
        {
            await _context.Projects.AddAsync(project, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Project project, CancellationToken ct = default)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid projectId, CancellationToken ct = default)
        {
            var project = await GetAsync(projectId, ct);
            if (project != null)
            {
                project.Disable();
                await UpdateAsync(project, ct);
            }
        }

        public async Task<bool> IsUserProjectOwnerAsync(Guid userId, Guid projectId, CancellationToken ct = default)
        {
            return await _context.Projects
                .IgnoreQueryFilters()
                .AnyAsync(p => p.Id == projectId && p.UserId == userId, ct);
        }

        public async Task<Project?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.Subdomain == subdomain, ct);
        }

        public async Task<bool> ExistsSubdomainAsync(string subdomain, CancellationToken ct = default)
        {
            return await _context.Projects
                .AnyAsync(p => p.Subdomain == subdomain, ct);
        }

        public async Task<Project?> GetByWhatsappPhoneNumberIdAsync(string phoneNumberId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(phoneNumberId)) return null;
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.WhatsappPhoneNumberId == phoneNumberId, ct);
        }

        public async Task<Project?> GetByWhatsappVerifyTokenAsync(string verifyToken, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(verifyToken)) return null;
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.WhatsappVerifyToken == verifyToken, ct);
        }
    }
}
