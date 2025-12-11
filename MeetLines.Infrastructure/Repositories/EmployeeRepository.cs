using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;

namespace MeetLines.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly MeetLinesPgDbContext _context;
        private readonly ITenantQueryFilter _tenantFilter;

        public EmployeeRepository(MeetLinesPgDbContext context, ITenantQueryFilter tenantFilter)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantFilter = tenantFilter ?? throw new ArgumentNullException(nameof(tenantFilter));
        }

        public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (employee == null) return null;
            if (tenantId.HasValue && employee.ProjectId != tenantId.Value) return null;
            return employee;
        }

        public async Task<Employee?> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Username == username, ct);
            if (employee == null) return null;
            if (tenantId.HasValue && employee.ProjectId != tenantId.Value) return null;
            return employee;
        }

        public async Task<IEnumerable<Employee>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            if (tenantId.HasValue && tenantId.Value != projectId)
                return Enumerable.Empty<Employee>();
            return await _context.Employees
                .Where(e => e.ProjectId == projectId)
                .OrderBy(e => e.Name)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Employee>> GetActiveByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            if (tenantId.HasValue && tenantId.Value != projectId)
                return Enumerable.Empty<Employee>();
            return await _context.Employees
                .Where(e => e.ProjectId == projectId && e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Employee>> GetByAreaAsync(Guid projectId, string area, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            if (tenantId.HasValue && tenantId.Value != projectId)
                return Enumerable.Empty<Employee>();
            return await _context.Employees
                .Where(e => e.ProjectId == projectId && e.Area == area && e.IsActive)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Employee employee, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            if (tenantId.HasValue && employee.ProjectId != tenantId.Value)
                throw new InvalidOperationException("Cannot add employee to a project outside the current tenant.");
            await _context.Employees.AddAsync(employee, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            if (tenantId.HasValue && employee.ProjectId != tenantId.Value)
                throw new InvalidOperationException("Cannot update employee of a project outside the current tenant.");
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync(ct);
        }
    }
}
