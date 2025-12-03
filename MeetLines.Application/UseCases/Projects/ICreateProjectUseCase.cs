using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Use case para crear un nuevo proyecto/empresa
    /// </summary>
    public interface ICreateProjectUseCase
    {
        Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            CreateProjectRequest request,
            CancellationToken ct = default);
    }
}
