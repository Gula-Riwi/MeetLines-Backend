using System;
using System.IO;
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
            Stream? profilePhotoStream = null,
            string? profilePhotoFileName = null,
            CancellationToken ct = default);
    }
}
