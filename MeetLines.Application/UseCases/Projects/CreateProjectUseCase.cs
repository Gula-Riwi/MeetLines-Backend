using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Application.Services.Interfaces;
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
        private readonly MeetLines.Application.Services.Interfaces.IEmailService _emailService;
        private readonly ISaasUserRepository _userRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public CreateProjectUseCase(
            IProjectRepository projectRepository,
            ISubscriptionRepository subscriptionRepository,
            IConfiguration configuration,
            MeetLines.Application.Services.Interfaces.IEmailService emailService,
            ISaasUserRepository userRepository,
            ICloudinaryService cloudinaryService)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }

        public async Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            CreateProjectRequest request,
            Stream? profilePhotoStream = null,
            string? profilePhotoFileName = null,
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

                // Generar subdominio: slug(nombre) + "-" + suffix(4 chars)
                var slug = SlugGenerator.Generate(request.Name);
                var suffix = GenerateUniqueSuffix();
                string subdomain = $"{slug}-{suffix}";

                // Validar formato
                if (!SubdomainValidator.IsValid(subdomain, out var validationError))
                {
                    return Result<ProjectResponse>.Fail($"Invalid subdomain: {validationError}");
                }

                // Validar unicidad
                // Validar unicidad y reintentar si es necesario (casos extremadamente raros)
                int retryCount = 0;
                while (await _projectRepository.ExistsSubdomainAsync(subdomain, ct) && retryCount < 5)
                {
                   suffix = GenerateUniqueSuffix();
                   subdomain = $"{slug}-{suffix}";
                   retryCount++;
                }

                if (await _projectRepository.ExistsSubdomainAsync(subdomain, ct))
                {
                    return Result<ProjectResponse>.Fail($"Unable to generate a unique subdomain for '{request.Name}'. Please try again.");
                }

                // Crear el proyecto
                var project = new Domain.Entities.Project(
                    userId,
                    request.Name,
                    subdomain,
                    request.Industry,
                    request.Description,
                    request.Address,
                    request.City,
                    request.Country,
                    request.Latitude,
                    request.Longitude);

                // Subir foto de perfil si se proporciona
                if (profilePhotoStream != null && !string.IsNullOrWhiteSpace(profilePhotoFileName))
                {
                    var (url, publicId) = await _cloudinaryService.UploadPhotoAsync(profilePhotoStream, profilePhotoFileName);
                    project.UpdateProfilePhoto(url, publicId);
                }
                // Mantener compatibilidad con el campo ProfilePhotoUrl si se proporciona (legacy)
                else if (!string.IsNullOrWhiteSpace(request.ProfilePhotoUrl))
                {
                    project.UpdateProfilePhoto(request.ProfilePhotoUrl, request.ProfilePhotoPublicId ?? string.Empty);
                }

                await _projectRepository.AddAsync(project, ct);



                // Fetch user to get email
                var user = await _userRepository.GetByIdAsync(userId, ct);
                if (user != null)
                {
                    await _emailService.SendProjectCreatedNotificationAsync(user.Email, user.Name, project.Name);
                }

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
            "free" => 1,
            "trial" => 1,
            "beginner" => 1,
            "intermediate" => 2,
            "complete" => int.MaxValue,
            _ => 1 // Default to 1 to avoid '0' quota blocking everything for unknown plans
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
                Address = project.Address,
                City = project.City,
                Country = project.Country,
                Latitude = project.Latitude,
                Longitude = project.Longitude,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                ProfilePhotoUrl = project.ProfilePhotoUrl
                , WhatsappPhoneNumberId = project.WhatsappPhoneNumberId
                , WhatsappForwardWebhook = project.WhatsappForwardWebhook
            };
        }


        private static string GenerateUniqueSuffix(int length = 4)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
