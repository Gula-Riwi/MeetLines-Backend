using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Use case para eliminar un proyecto
    /// </summary>
    public interface IDeleteProjectUseCase
    {
        Task<Result<bool>> ExecuteAsync(
            Guid userId,
            Guid projectId,
            CancellationToken ct = default);
    }
}
