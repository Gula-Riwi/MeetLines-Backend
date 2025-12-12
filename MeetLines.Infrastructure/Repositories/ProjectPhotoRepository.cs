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
    public class ProjectPhotoRepository : IProjectPhotoRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public ProjectPhotoRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ProjectPhoto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.ProjectPhotos
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IEnumerable<ProjectPhoto>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.ProjectPhotos
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.IsMain) // Main photo first
                .ThenByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<int> CountByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.ProjectPhotos
                .CountAsync(x => x.ProjectId == projectId, ct);
        }

        public async Task AddAsync(ProjectPhoto photo, CancellationToken ct = default)
        {
            _context.ProjectPhotos.Add(photo);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(ProjectPhoto photo, CancellationToken ct = default)
        {
            _context.ProjectPhotos.Remove(photo);
            await _context.SaveChangesAsync(ct);
        }
        
        public async Task<ProjectPhoto?> GetMainPhotoAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.ProjectPhotos
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.IsMain, ct);
        }
    }
}
