using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;

using MeetLines.Application.UseCases.Projects.Interfaces;

namespace MeetLines.Application.UseCases.Projects
{
    public class DeleteProjectPhotoUseCase : IDeleteProjectPhotoUseCase
    {
        private readonly IProjectPhotoRepository _photoRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public DeleteProjectPhotoUseCase(
            IProjectPhotoRepository photoRepository,
            ICloudinaryService cloudinaryService)
        {
            _photoRepository = photoRepository ?? throw new ArgumentNullException(nameof(photoRepository));
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }

        public async Task<Result<bool>> ExecuteAsync(Guid projectId, Guid photoId, CancellationToken ct = default)
        {
            var photo = await _photoRepository.GetByIdAsync(photoId, ct);

            if (photo == null) return Result<bool>.Fail("Photo not found");
            
            if (photo.ProjectId != projectId) return Result<bool>.Fail("Photo does not belong to this project");

            // 1. Delete from Cloudinary
            if (!string.IsNullOrEmpty(photo.PublicId))
            {
                await _cloudinaryService.DeletePhotoAsync(photo.PublicId);
            }

            // 2. Delete from DB
            await _photoRepository.DeleteAsync(photo, ct);

            return Result<bool>.Ok(true);
        }
    }
}
