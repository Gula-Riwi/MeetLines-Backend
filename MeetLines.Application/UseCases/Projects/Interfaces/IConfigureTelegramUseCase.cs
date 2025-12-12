using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;

namespace MeetLines.Application.UseCases.Projects.Interfaces
{
    public interface IConfigureTelegramUseCase
    {
        Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            Guid projectId,
            ConfigureTelegramRequest request,
            CancellationToken ct = default);
    }
}
