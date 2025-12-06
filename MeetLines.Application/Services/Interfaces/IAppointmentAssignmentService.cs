using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IAppointmentAssignmentService
    {
        /// <summary>
        /// Finds an available employee in the specified area for a project.
        /// Logic: Round Robin or Random active employee.
        /// </summary>
        Task<Employee?> FindAvailableEmployeeAsync(Guid projectId, string area, CancellationToken ct = default);
    }
}
