using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Domain.Repositories;
using MeetLines.Domain.ValueObjects;
using SharedKernel.Utilities;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Implementación del use case para crear un nuevo proyecto
    /// Valida que el usuario no haya excedido el límite de su plan
    /// Genera y valida el subdominio único
    /// </summary>
    public class CreateProjectUseCase : ICreateProjectUseCase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IConfiguration _configuration;

        public CreateProjectUseCase(
            IProjectRepository projectRepository,
            ISubscriptionRepository subscriptionRepository,
            IConfiguration configuration)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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

                // Generar o validar subdominio
                string subdomain;
                if (!string.IsNullOrWhiteSpace(request.Subdomain))
                {
                    subdomain = request.Subdomain.ToLowerInvariant();
                }
                else
                {
                    subdomain = SlugGenerator.Generate(request.Name);
                }

                // Validar formato
                if (!SubdomainValidator.IsValid(subdomain, out var validationError))
                {
                    return Result<ProjectResponse>.Fail($"Invalid subdomain: {validationError}");
                }

                // Validar unicidad
                if (await _projectRepository.ExistsSubdomainAsync(subdomain, ct))
                {
                    // Si fue generado automáticamente, intentar agregar sufijo numérico
                    if (string.IsNullOrWhiteSpace(request.Subdomain))
                    {
                        int suffix = 1;
                        string originalSubdomain = subdomain;
                        while (await _projectRepository.ExistsSubdomainAsync(subdomain, ct) && suffix < 100)
                        {
                            subdomain = $"{originalSubdomain}-{suffix}";
                            suffix++;
                        }
                        
                        if (await _projectRepository.ExistsSubdomainAsync(subdomain, ct))
                        {
                            return Result<ProjectResponse>.Fail($"Cannot generate a unique subdomain for '{request.Name}'. Please specify a custom subdomain.");
                        }
                    }
                    else
                    {
                        return Result<ProjectResponse>.Fail($"Subdomain '{subdomain}' is already taken.");
                    }
                }

                // Crear el proyecto
                var project = new Domain.Entities.Project(
                    userId,
                    request.Name,
                    subdomain,
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

        private ProjectResponse MapToResponse(Domain.Entities.Project project)
        {
            var baseDomain = _configuration["Multitenancy:BaseDomain"] ?? "meet-lines.com";
            var protocol = _configuration["Multitenancy:Protocol"] ?? "https";
            var fullUrl = $"{protocol}://{project.Subdomain}.{baseDomain}";

            return new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Subdomain = project.Subdomain,
                FullUrl = fullUrl,
                Industry = project.Industry,
                Description = project.Description,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }
    }
}
