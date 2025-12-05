using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IAppointmentRepository
    {
        Task<Appointment> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<Appointment>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
        Task<IEnumerable<Appointment>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken ct = default);
        Task AddAsync(Appointment appointment, CancellationToken ct = default);
        Task UpdateAsync(Appointment appointment, CancellationToken ct = default);
    }
}
