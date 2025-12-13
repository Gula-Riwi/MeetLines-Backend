using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Domain.Repositories;

using MeetLines.Application.UseCases.Projects.Interfaces;

namespace MeetLines.Application.UseCases.Projects
{
    public class GetProjectPhotosUseCase : IGetProjectPhotosUseCase
    {
        private readonly IProjectPhotoRepository _photoRepository;

        public GetProjectPhotosUseCase(IProjectPhotoRepository photoRepository)
        {
            _photoRepository = photoRepository ?? throw new ArgumentNullException(nameof(photoRepository));
        }

        public async Task<Result<IEnumerable<PhotoDto>>> ExecuteAsync(Guid projectId, CancellationToken ct = default)
        {
            var photos = await _photoRepository.GetByProjectIdAsync(projectId, ct);
            
            var dtos = photos.Select(p => new PhotoDto
            {
                Id = p.Id,
                Url = p.Url,
                IsMain = p.IsMain,
                CreatedAt = p.CreatedAt
            });

            return Result<IEnumerable<PhotoDto>>.Ok(dtos);
        }
    }
}
