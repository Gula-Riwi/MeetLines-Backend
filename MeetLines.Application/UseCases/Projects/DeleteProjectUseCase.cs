using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Implementación del use case para eliminar un proyecto
    /// </summary>
    public class DeleteProjectUseCase : IDeleteProjectUseCase
    {
        private readonly IProjectRepository _projectRepository;

        public DeleteProjectUseCase(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<Result<bool>> ExecuteAsync(
            Guid userId,
            Guid projectId,
            CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return Result<bool>.Fail("User ID is invalid");

            if (projectId == Guid.Empty)
                return Result<bool>.Fail("Project ID is invalid");

            try
            {
                var project = await _projectRepository.GetAsync(projectId, ct);
                if (project == null)
                    return Result<bool>.Fail("Project not found");

                var isOwner = await _projectRepository.IsUserProjectOwnerAsync(userId, projectId, ct);
                if (!isOwner)
                    return Result<bool>.Fail("You do not have permission to delete this project");

                await _projectRepository.DeleteAsync(projectId, ct);
                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
