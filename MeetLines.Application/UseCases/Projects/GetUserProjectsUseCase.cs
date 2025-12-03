using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Implementación del use case para obtener los proyectos del usuario
    /// </summary>
    public class GetUserProjectsUseCase : IGetUserProjectsUseCase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public GetUserProjectsUseCase(
            IProjectRepository projectRepository,
            ISubscriptionRepository subscriptionRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        }

        public async Task<Result<UserProjectsResponse>> ExecuteAsync(Guid userId, CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return Result<UserProjectsResponse>.Fail("User ID is invalid");

            try
            {
                var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, ct);
                if (subscription == null)
                    return Result<UserProjectsResponse>.Fail("No active subscription found for this user");

                var projects = await _projectRepository.GetByUserAsync(userId, ct);
                var projectList = projects.ToList();

                int maxProjects = GetMaxProjectsByPlan(subscription.Plan);
                int currentCount = projectList.Count;

                return Result<UserProjectsResponse>.Ok(new UserProjectsResponse
                {
                    Plan = subscription.Plan,
                    MaxProjects = maxProjects,
                    CurrentProjects = currentCount,
                    CanCreateMore = currentCount < maxProjects,
                    Projects = projectList
                        .Select(MapToResponse)
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                return Result<UserProjectsResponse>.Fail($"An unexpected error occurred: {ex.Message}");
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
