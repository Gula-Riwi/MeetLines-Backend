using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Channels;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Channels
{
    public class GetProjectChannelsUseCase : IGetProjectChannelsUseCase
    {
        private readonly IChannelRepository _channelRepository;
        private readonly IProjectRepository _projectRepository;

        public GetProjectChannelsUseCase(IChannelRepository channelRepository, IProjectRepository projectRepository)
        {
            _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<Result<IEnumerable<ChannelDto>>> ExecuteAsync(Guid userId, Guid projectId, CancellationToken ct = default)
        {
            // Verify project access
            // Note: Currently simple ownership check. Should be robust permission check if we have roles.
            var project = await _projectRepository.GetAsync(projectId, ct);
            if (project == null) return Result<IEnumerable<ChannelDto>>.Fail("Project not found");
            
            // Allow if user is owner. (For now simple check)
            if (project.UserId != userId) return Result<IEnumerable<ChannelDto>>.Fail("Unauthorized");

            var channels = await _channelRepository.GetByProjectIdAsync(projectId, ct);
            
            var dtos = channels.Select(c => new ChannelDto
            {
                Id = c.Id,
                ProjectId = c.ProjectId,
                Type = c.Type,
                Verified = c.Verified,
                CreatedAt = c.CreatedAt
            });

            return Result<IEnumerable<ChannelDto>>.Ok(dtos);
        }
    }
}
