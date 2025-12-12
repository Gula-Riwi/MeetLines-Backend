using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.Projects;

namespace MeetLines.Application.UseCases.Projects
{
    public interface IUploadProjectPhotoUseCase
    {
        Task<PhotoDto> ExecuteAsync(Guid userId, Guid projectId, Stream fileStream, string fileName, CancellationToken ct = default);
    }
}
