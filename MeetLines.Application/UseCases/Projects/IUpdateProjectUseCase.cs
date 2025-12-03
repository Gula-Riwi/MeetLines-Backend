using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Use case para actualizar un proyecto
    /// </summary>
    public interface IUpdateProjectUseCase
    {
        Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            Guid projectId,
            UpdateProjectRequest request,
            CancellationToken ct = default);
    }
}
