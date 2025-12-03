using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Use case para obtener un proyecto por ID
    /// </summary>
    public interface IGetProjectByIdUseCase
    {
        Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            Guid projectId,
            CancellationToken ct = default);
    }
}
