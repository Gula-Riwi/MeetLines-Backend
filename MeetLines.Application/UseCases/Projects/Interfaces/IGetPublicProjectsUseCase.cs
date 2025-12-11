using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;

namespace MeetLines.Application.UseCases.Projects
{
    public interface IGetPublicProjectsUseCase
    {
        Task<Result<IEnumerable<ProjectPublicSummaryDto>>> ExecuteAsync(double? latitude = null, double? longitude = null, CancellationToken ct = default);
    }
}
