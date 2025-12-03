using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Implementación del use case para actualizar un proyecto
    /// </summary>
    public class UpdateProjectUseCase : IUpdateProjectUseCase
    {
        private readonly IProjectRepository _projectRepository;

        public UpdateProjectUseCase(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            Guid projectId,
            UpdateProjectRequest request,
            CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return Result<ProjectResponse>.Fail("User ID is invalid");

            if (projectId == Guid.Empty)
                return Result<ProjectResponse>.Fail("Project ID is invalid");

            if (request == null)
                return Result<ProjectResponse>.Fail("Request cannot be null");

            try
            {
                var project = await _projectRepository.GetAsync(projectId, ct);
                if (project == null)
                    return Result<ProjectResponse>.Fail("Project not found");

                var isOwner = await _projectRepository.IsUserProjectOwnerAsync(userId, projectId, ct);
                if (!isOwner)
                    return Result<ProjectResponse>.Fail("You do not have permission to update this project");

                project.UpdateDetails(request.Name, request.Industry, request.Description);
                await _projectRepository.UpdateAsync(project, ct);

                return Result<ProjectResponse>.Ok(MapToResponse(project));
            }
            catch (InvalidOperationException ex)
            {
                return Result<ProjectResponse>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<ProjectResponse>.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }

        private ProjectResponse MapToResponse(Domain.Entities.Project project) => new()
        {
            Id = project.Id,
            Name = project.Name,
            Industry = project.Industry,
            Description = project.Description,
            Status = project.Status,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}
