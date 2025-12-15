using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Projects
{
    public class UploadProjectProfilePhotoUseCase : IUploadProjectProfilePhotoUseCase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public UploadProjectProfilePhotoUseCase(
            IProjectRepository projectRepository,
            ICloudinaryService cloudinaryService)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }

        public async Task<string> ExecuteAsync(Guid userId, Guid projectId, Stream fileStream, string fileName, CancellationToken ct = default)
        {
            var project = await _projectRepository.GetAsync(projectId, ct);
            if (project == null)
            {
               throw new ArgumentException("Project not found");
            }

            if (project.UserId != userId)
            {
                throw new UnauthorizedAccessException("User is not the owner of this project");
            }

            // Upload new photo
            var (url, publicId) = await _cloudinaryService.UploadPhotoAsync(fileStream, fileName);

            // Delete old photo if exists
            if (!string.IsNullOrEmpty(project.ProfilePhotoPublicId))
            {
                await _cloudinaryService.DeletePhotoAsync(project.ProfilePhotoPublicId);
            }

            // Update project
            project.UpdateProfilePhoto(url, publicId);
            await _projectRepository.UpdateAsync(project, ct);

            return url;
        }
    }
}
