using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Appointment?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
        Task<Appointment?> FindDuplicateAsync(Guid projectId, Guid appUserId, DateTimeOffset startTime, CancellationToken ct = default);
        Task<IEnumerable<Appointment>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
        Task<IEnumerable<Appointment>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken ct = default);
        Task<IEnumerable<Appointment>> GetByAppUserIdAsync(Guid appUserId, CancellationToken ct = default);
        
        // Dashboard Stats
        Task<decimal> GetTotalSalesAsync(Guid projectId, DateTimeOffset starDate, DateTimeOffset endDate, CancellationToken ct = default);
        Task<IEnumerable<Appointment>> GetRecentAppointmentsAsync(Guid projectId, int limit, CancellationToken ct = default);
        Task<IEnumerable<Appointment>> GetEmployeeTasksAsync(Guid projectId, Guid? employeeId, DateTimeOffset? fromDate, CancellationToken ct = default);
        
        Task AddAsync(Appointment appointment, CancellationToken ct = default);
        Task UpdateAsync(Appointment appointment, CancellationToken ct = default);
    }
}
