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

        public async Task<IEnumerable<Appointment>> GetByAppUserIdAsync(Guid appUserId, CancellationToken ct = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .Include(a => a.Project)
                .Where(x => x.AppUserId == appUserId)
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

        public async Task<IEnumerable<Appointment>> GetInactiveCustomersAsync(Guid projectId, DateTimeOffset sinceDate, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            
            // Logic: 
            // 1. Group by AppUser
            // 2. Select Max(StartTime)
            // 3. Filter where Max < sinceDate AND no Future appointments
            
            // Note: Use client-side evaluation for complex grouping if EF Core limitation requires it, 
            // but for performance, we prefer server-side.
            // Let's get "Latest Appt" for each user in the project. This can be heavy, but necessary.
            
            var userLastAppointments = await _context.Appointments
                .Where(x => x.ProjectId == projectId && x.Status != "cancelled")
                .GroupBy(x => x.AppUserId)
                .Select(g => new 
                { 
                    AppUserId = g.Key, 
                    LastDate = g.Max(x => x.StartTime),
                    FutureCount = g.Count(x => x.StartTime > now)
                })
                .Where(x => x.LastDate < sinceDate && x.FutureCount == 0)
                .ToListAsync(ct);

            // Now hydrate the actual appointment objects (the latest one)
            var userIds = userLastAppointments.Select(x => x.AppUserId).ToList();
            
            // We return "The last appointment" because it contains the User/Employee data we might need (e.g. "Last time you saw John")
            // Fetch latest appt for these users
             var appointments = await _context.Appointments
                .Include(a => a.AppUser)
                .Include(a => a.Project)
                .Where(x => userIds.Contains(x.AppUserId))
                // We need to pick one per user (the latest)
                // Doing this in memory after fetching matches is safer for EF translation
                .ToListAsync(ct);

            return appointments
                .GroupBy(a => a.AppUserId)
                .Select(g => g.OrderByDescending(x => x.StartTime).First())
                .Where(x => x.StartTime < sinceDate) // Double check
                .ToList();
        }
    }
}
