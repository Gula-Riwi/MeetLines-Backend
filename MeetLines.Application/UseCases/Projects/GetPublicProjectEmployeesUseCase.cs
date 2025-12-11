using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Employees;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Projects
{
    public class GetPublicProjectEmployeesUseCase : IGetPublicProjectEmployeesUseCase
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IProjectRepository _projectRepository;

        public GetPublicProjectEmployeesUseCase(
            IEmployeeRepository employeeRepository,
            IProjectRepository projectRepository)
        {
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<Result<IEnumerable<EmployeePublicDto>>> ExecuteAsync(Guid projectId, CancellationToken ct = default)
        {
            // Verify project exists (optional but good practice)
            var project = await _projectRepository.GetAsync(projectId, ct);
            if (project == null)
            {
                return Result<IEnumerable<EmployeePublicDto>>.Fail("Project not found");
            }

            // Get active employees
            var employees = await _employeeRepository.GetActiveByProjectIdAsync(projectId, ct);

            var dtos = employees.Select(e => new EmployeePublicDto
            {
                Name = e.Name,
                Area = e.Area
            }).ToList();

            return Result<IEnumerable<EmployeePublicDto>>.Ok(dtos);
        }
    }
}
