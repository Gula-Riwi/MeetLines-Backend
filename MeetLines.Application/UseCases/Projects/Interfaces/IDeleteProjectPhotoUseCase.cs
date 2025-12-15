using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;

namespace MeetLines.Application.UseCases.Projects.Interfaces
{
    public interface IDeleteProjectPhotoUseCase
    {
        Task<Result<bool>> ExecuteAsync(Guid projectId, Guid photoId, CancellationToken ct = default);
    }
}
