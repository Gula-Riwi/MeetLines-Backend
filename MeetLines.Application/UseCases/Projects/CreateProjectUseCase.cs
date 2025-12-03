using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Implementación del use case para crear un nuevo proyecto
    /// Valida que el usuario no haya excedido el límite de su plan
    /// </summary>
    public class CreateProjectUseCase : ICreateProjectUseCase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public CreateProjectUseCase(
            IProjectRepository projectRepository,
            ISubscriptionRepository subscriptionRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        }

        public async Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            CreateProjectRequest request,
            CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return Result<ProjectResponse>.Fail("User ID is invalid");

            if (request == null)
                return Result<ProjectResponse>.Fail("Request cannot be null");

            try
            {
                // Obtener la suscripción activa del usuario
                var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, ct);
                if (subscription == null)
                    return Result<ProjectResponse>.Fail("No active subscription found for this user");

                // Validar límite de proyectos según el plan
                int maxProjects = GetMaxProjectsByPlan(subscription.Plan);
                int currentCount = await _projectRepository.GetActiveCountByUserAsync(userId, ct);

                if (currentCount >= maxProjects)
                {
                    return Result<ProjectResponse>.Fail(
                        $"Cannot create more projects. Your plan ({subscription.Plan}) allows a maximum of {maxProjects} project(s). " +
                        $"Current projects: {currentCount}");
                }

                // Crear el proyecto
                var project = new Domain.Entities.Project(
                    userId,
                    request.Name,
                    request.Industry,
                    request.Description);

                await _projectRepository.AddAsync(project, ct);

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

        private int GetMaxProjectsByPlan(string plan) => plan?.ToLower() switch
        {
            "beginner" => 1,
            "intermediate" => 2,
            "complete" => int.MaxValue,
            _ => 0
        };

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
