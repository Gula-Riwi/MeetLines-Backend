using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Projects
{
    public class ConfigureWhatsappUseCase : IConfigureWhatsappUseCase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IConfiguration _configuration;

        public ConfigureWhatsappUseCase(
            IProjectRepository projectRepository,
            IConfiguration configuration)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            Guid projectId,
            ConfigureWhatsappRequest request,
            CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return Result<ProjectResponse>.Fail("User ID is invalid");

            var project = await _projectRepository.GetAsync(projectId, ct);
            if (project == null)
                return Result<ProjectResponse>.Fail("Project not found");

            if (project.UserId != userId)
                return Result<ProjectResponse>.Fail("Unauthorized access to project");

            // Get webhook from Environment/Configuration
            var waForwardWebhook = _configuration["WHATSAPP_FORWARD_WEBHOOK"];
            
            // Assuming the webhook is mandatory if we are configuring WhatsApp. 
            // If it's optional in env, we pass null/empty.
            // But per user request: "el WHATSAPP_FORWARD_WEBHOOK y que solo se ponga cuando uno haga un patch"
            
            project.UpdateWhatsappIntegration(
                request.WhatsappVerifyToken,
                request.WhatsappPhoneNumberId,
                request.WhatsappAccessToken,
                waForwardWebhook
            );

            await _projectRepository.UpdateAsync(project, ct);

            return Result<ProjectResponse>.Ok(MapToResponse(project));
        }

        private ProjectResponse MapToResponse(Domain.Entities.Project project)
        {
            // Reuse mapping logic or duplicate it for now (simpler than refactoring entire mapper service yet)
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
                UpdatedAt = project.UpdatedAt,
                ProfilePhotoUrl = project.ProfilePhotoUrl,
                WhatsappPhoneNumberId = project.WhatsappPhoneNumberId,
                WhatsappForwardWebhook = project.WhatsappForwardWebhook
            };
        }
    }
}
