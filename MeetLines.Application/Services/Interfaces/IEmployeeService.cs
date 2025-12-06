using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Employees;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<Result<EmployeeResponse>> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken ct = default);
        Task<Result<IEnumerable<EmployeeResponse>>> GetEmployeesByProjectAsync(Guid projectId, CancellationToken ct = default);
    }
}
