using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Use case para obtener todos los proyectos del usuario
    /// </summary>
    public interface IGetUserProjectsUseCase
    {
        Task<Result<UserProjectsResponse>> ExecuteAsync(
            Guid userId,
            CancellationToken ct = default);
    }
}
