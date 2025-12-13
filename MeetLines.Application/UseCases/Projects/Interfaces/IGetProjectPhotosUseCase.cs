using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;

namespace MeetLines.Application.UseCases.Projects.Interfaces
{
    public interface IGetProjectPhotosUseCase
    {
        Task<Result<IEnumerable<PhotoDto>>> ExecuteAsync(Guid projectId, CancellationToken ct = default);
    }
}
