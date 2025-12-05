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
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public AppointmentRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Appointment> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task<IEnumerable<Appointment>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.Appointments
                .Where(a => a.ProjectId == projectId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Appointment>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken ct = default)
        {
            return await _context.Appointments
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Appointment appointment, CancellationToken ct = default)
        {
            await _context.Appointments.AddAsync(appointment, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Appointment appointment, CancellationToken ct = default)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync(ct);
        }
    }
}
