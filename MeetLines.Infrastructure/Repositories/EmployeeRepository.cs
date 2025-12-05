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
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public EmployeeRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<Employee?> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Username == username, ct);
        }

        public async Task<IEnumerable<Employee>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.Employees
                .Where(e => e.ProjectId == projectId)
                .OrderBy(e => e.Name)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Employee>> GetByAreaAsync(Guid projectId, string area, CancellationToken ct = default)
        {
            return await _context.Employees
                .Where(e => e.ProjectId == projectId && e.Area == area && e.IsActive)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Employee employee, CancellationToken ct = default)
        {
            await _context.Employees.AddAsync(employee, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync(ct);
        }
    }
}
