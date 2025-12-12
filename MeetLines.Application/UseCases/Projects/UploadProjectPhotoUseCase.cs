using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Projects
{
    public class UploadProjectPhotoUseCase : IUploadProjectPhotoUseCase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectPhotoRepository _photoRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public UploadProjectPhotoUseCase(
            IProjectRepository projectRepository, 
            IProjectPhotoRepository photoRepository,
            ICloudinaryService cloudinaryService)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _photoRepository = photoRepository ?? throw new ArgumentNullException(nameof(photoRepository));
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }

        public async Task<PhotoDto> ExecuteAsync(Guid userId, Guid projectId, Stream fileStream, string fileName, CancellationToken ct = default)
        {
            // 1. Validar existencia y propiedad del proyecto
            var project = await _projectRepository.GetAsync(projectId, ct);
            if (project == null)
            {
                throw new ArgumentException("Project not found");
            }

            if (project.UserId != userId)
            {
                throw new UnauthorizedAccessException("User is not the owner of this project");
            }

            // 2. Validar límite de 10 fotos
            var currentCount = await _photoRepository.CountByProjectIdAsync(projectId, ct);
            if (currentCount >= 10)
            {
                throw new InvalidOperationException("Project has reached the maximum limit of 10 photos.");
            }

            // 3. Subir a Cloudinary
            var (url, publicId) = await _cloudinaryService.UploadPhotoAsync(fileStream, fileName);

            // 4. Crear entidad (si es la primera, es Main)
            var isMain = currentCount == 0;
            var photo = new ProjectPhoto(projectId, url, publicId, isMain);

            // 5. Guardar en BD
            await _photoRepository.AddAsync(photo, ct);

            // 6. Retornar DTO
            return new PhotoDto
            {
                Id = photo.Id,
                Url = photo.Url,
                IsMain = photo.IsMain,
                CreatedAt = photo.CreatedAt
            };
        }
    }
}
