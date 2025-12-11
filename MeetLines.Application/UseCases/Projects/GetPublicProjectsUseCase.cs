using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;
using MeetLines.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace MeetLines.Application.UseCases.Projects
{
    public class GetPublicProjectsUseCase : IGetPublicProjectsUseCase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IConfiguration _configuration;

        public GetPublicProjectsUseCase(IProjectRepository projectRepository, IConfiguration configuration)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Result<IEnumerable<ProjectPublicSummaryDto>>> ExecuteAsync(CancellationToken ct = default)
        {
            var projects = await _projectRepository.GetAllAsync(ct);
            var dtos = projects.Select(MapToPublicDto).ToList();
            return Result<IEnumerable<ProjectPublicSummaryDto>>.Ok(dtos);
        }

        private ProjectPublicSummaryDto MapToPublicDto(Domain.Entities.Project project)
        {
            return new ProjectPublicSummaryDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description ?? string.Empty,
                Industry = project.Industry ?? string.Empty
            };
        }
    }
}
