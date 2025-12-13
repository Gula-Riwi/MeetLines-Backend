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

        public async Task<Appointment?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

        }

        public async Task<Appointment?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _context.Appointments
                .Include(a => a.AppUser)
                .Include(a => a.Project)
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .AsNoTracking() // We will use UpdateAsync to save changes
                .FirstOrDefaultAsync(x => x.Id == id, ct);

        }

        public async Task<Appointment?> FindDuplicateAsync(Guid projectId, Guid appUserId, DateTimeOffset startTime, CancellationToken ct = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId && 
                            x.AppUserId == appUserId && 
                            x.StartTime == startTime && 
                            x.Status != "cancelled")
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IEnumerable<Appointment>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.StartTime)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Appointment>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken ct = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.StartTime)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Appointment appointment, CancellationToken ct = default)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Appointment appointment, CancellationToken ct = default)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<decimal> GetTotalSalesAsync(Guid projectId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken ct = default)
        {
            return await _context.Appointments
                .Where(x => x.ProjectId == projectId && 
                            x.Status == "completed" && 
                            x.StartTime >= startDate && 
                            x.StartTime <= endDate)
                .SumAsync(x => x.PriceSnapshot, ct);
        }

        public async Task<IEnumerable<Appointment>> GetRecentAppointmentsAsync(Guid projectId, int limit, CancellationToken ct = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.StartTime)
                .Take(limit)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Appointment>> GetEmployeeTasksAsync(Guid projectId, Guid? employeeId, DateTimeOffset? fromDate, DateTimeOffset? toDate = null, CancellationToken ct = default)
        {
            var query = _context.Appointments
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId);

            if (employeeId.HasValue)
            {
                query = query.Where(x => x.EmployeeId == employeeId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.StartTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.StartTime <= toDate.Value);
            }

            return await query
                .OrderByDescending(x => x.StartTime)
                .ToListAsync(ct);
        }
    }
}
