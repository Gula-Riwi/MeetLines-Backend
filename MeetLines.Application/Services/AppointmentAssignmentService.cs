using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class AppointmentAssignmentService : IAppointmentAssignmentService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly Random _random;

        public AppointmentAssignmentService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _random = new Random();
        }

        public async Task<Employee?> FindAvailableEmployeeAsync(Guid projectId, string area, CancellationToken ct = default)
        {
            // 1. Get all active employees in the area
            var employees = await _employeeRepository.GetByAreaAsync(projectId, area, ct);
            var employeeList = employees.ToList();

            if (!employeeList.Any())
            {
                return null;
            }

            // 2. Simple Random Assignment for now (Can be upgraded to Round Robin with Redis/DB tracking)
            var index = _random.Next(employeeList.Count);
            return employeeList[index];
        }
    }
}
