using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Projects
{
    /// <summary>
    /// Implementación del use case para obtener un proyecto por ID
    /// </summary>
    public class GetProjectByIdUseCase : IGetProjectByIdUseCase
    {
        private readonly IProjectRepository _projectRepository;

        public GetProjectByIdUseCase(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            Guid projectId,
            CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return Result<ProjectResponse>.Fail("User ID is invalid");

            if (projectId == Guid.Empty)
                return Result<ProjectResponse>.Fail("Project ID is invalid");

            try
            {
                var project = await _projectRepository.GetAsync(projectId, ct);
                if (project == null)
                    return Result<ProjectResponse>.Fail("Project not found");

                var isOwner = await _projectRepository.IsUserProjectOwnerAsync(userId, projectId, ct);
                if (!isOwner)
                    return Result<ProjectResponse>.Fail("You do not have permission to access this project");

                return Result<ProjectResponse>.Ok(MapToResponse(project));
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
            Subdomain = project.Subdomain,
            Industry = project.Industry,
            Description = project.Description,
            Address = project.Address,
            City = project.City,
            Country = project.Country,
            Latitude = project.Latitude,
            Longitude = project.Longitude,
            Status = project.Status,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            ProfilePhotoUrl = project.ProfilePhotoUrl,
            WhatsappPhoneNumberId = project.WhatsappPhoneNumberId,
            WhatsappForwardWebhook = project.WhatsappForwardWebhook,
            TelegramBotToken = project.TelegramBotToken,
            TelegramBotUsername = project.TelegramBotUsername,
            TelegramForwardWebhook = project.TelegramForwardWebhook
        };
}
}
