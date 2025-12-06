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
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly MeetLinesPgDbContext _context;
        private readonly ITenantQueryFilter _tenantFilter;

        public AppointmentRepository(MeetLinesPgDbContext context, ITenantQueryFilter tenantFilter)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantFilter = tenantFilter ?? throw new ArgumentNullException(nameof(tenantFilter));
        }

        public async Task<Appointment?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (appointment == null) return null;
            if (tenantId.HasValue && appointment.ProjectId != tenantId.Value) return null;
            return appointment;
        }

        public async Task<IEnumerable<Appointment>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            if (tenantId.HasValue && tenantId.Value != projectId)
                return Enumerable.Empty<Appointment>();
            return await _context.Appointments
                .Where(a => a.ProjectId == projectId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Appointment>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            var query = _context.Appointments.AsQueryable();
            if (tenantId.HasValue)
                query = query.Where(a => a.ProjectId == tenantId.Value);
            return await query
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Appointment appointment, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            if (tenantId.HasValue && appointment.ProjectId != tenantId.Value)
                throw new InvalidOperationException("Cannot add appointment to a project outside the current tenant.");
            await _context.Appointments.AddAsync(appointment, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Appointment appointment, CancellationToken ct = default)
        {
            var tenantId = _tenantFilter.GetCurrentTenantId();
            if (tenantId.HasValue && appointment.ProjectId != tenantId.Value)
                throw new InvalidOperationException("Cannot update appointment of a project outside the current tenant.");
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync(ct);
        }
    }
}
