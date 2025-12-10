using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;

namespace MeetLines.Application.UseCases.Projects
{
    public interface IConfigureWhatsappUseCase
    {
        Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            Guid projectId,
            ConfigureWhatsappRequest request,
            CancellationToken ct = default);
    }
}
