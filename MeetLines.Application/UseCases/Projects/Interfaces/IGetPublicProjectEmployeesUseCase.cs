using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Employees;

namespace MeetLines.Application.UseCases.Projects
{
    public interface IGetPublicProjectEmployeesUseCase
    {
        Task<Result<IEnumerable<EmployeePublicDto>>> ExecuteAsync(Guid projectId, CancellationToken ct = default);
    }
}
