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

        public async Task<IEnumerable<Project>> GetPublicProjectsByDistanceAsync(double? latitude, double? longitude, CancellationToken ct = default)
        {
            if (!latitude.HasValue || !longitude.HasValue)
            {
                return await GetAllAsync(ct);
            }

            // Haversine formula for distance in Kilometers
            // Formatos: 
            // - Nearest (Non-null lat/lng) sorted by distance ASC
            // - Remote (Null lat/lng) appended at the end? User asked for "y las que no tienen direccion porque son remotas"
            // Let's union or just sort.
            // Sorting: Remote first? Distance first? 
            // "busque las mas cercanas y las que no tienen direccion" likely means: Show me things close to me, AND things that are remote (available everywhere).
            
            // Logic:
            // 1. Calculate distance for all.
            // 2. Sort by: HasCoordinates? 
            // If I assume Remote (Null) means "Infinite Reach", maybe they should be mixed? 
            // But typically, a user wants "Pizza near me". If none, maybe "Frozen Pizza online". 
            // Let's sort: 
            // 1st Priority: Distance (ASC). Nulls Last? Nulls First?
            // If Nulls Last: Local stuff first, then remote stuff. (Recommended for "Near Me")
            
            // Raw SQL for performance and custom sorting
            var lat = latitude.Value;
            var lng = longitude.Value;

            // Using standard SQL with Haversine formula
            // 6371 * acos(cos(radians(lat)) * cos(radians(p.latitude)) * cos(radians(p.longitude) - radians(lng)) + sin(radians(lat)) * sin(radians(p.latitude)))
            
            var query = $@"
                SELECT *, 
                (
                    6371 * acos(
                        least(1.0, greatest(-1.0, 
                            cos(radians({lat})) * cos(radians(""latitude"")) * cos(radians(""longitude"") - radians({lng})) + 
                            sin(radians({lat})) * sin(radians(""latitude""))
                        ))
                    )
                ) as ""Distance""
                FROM projects
                WHERE status = 'active'
                ORDER BY 
                    CASE WHEN ""latitude"" IS DISTINCT FROM NULL AND ""longitude"" IS DISTINCT FROM NULL THEN 0 ELSE 1 END,
                    ""Distance"" ASC
            ";

            return await _context.Projects
                .FromSqlRaw(query)
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
