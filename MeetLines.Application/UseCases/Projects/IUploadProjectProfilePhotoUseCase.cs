using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.Application.UseCases.Projects
{
    public interface IUploadProjectProfilePhotoUseCase
    {
        Task<string> ExecuteAsync(Guid userId, Guid projectId, Stream fileStream, string fileName, CancellationToken ct = default);
    }
}
