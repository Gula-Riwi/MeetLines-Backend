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
    public class ServiceRepository : IServiceRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public ServiceRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Service?> GetAsync(int id, CancellationToken ct = default)
        {
            return await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IEnumerable<Service>> GetByProjectIdAsync(Guid projectId, bool activeOnly = true, CancellationToken ct = default)
        {
            var query = _context.Services
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId);

            if (activeOnly)
            {
                query = query.Where(x => x.IsActive);
            }

            return await query
                .OrderBy(x => x.Name)
                .ToListAsync(ct);
        }

        public async Task<Service> CreateAsync(Service service, CancellationToken ct = default)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync(ct);
            return service;
        }

        public async Task UpdateAsync(Service service, CancellationToken ct = default)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var service = await _context.Services.FindAsync(new object[] { id }, ct);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
