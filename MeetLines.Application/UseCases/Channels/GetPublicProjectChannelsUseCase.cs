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
    public interface IGetPublicProjectChannelsUseCase
    {
        Task<Result<IEnumerable<ChannelPublicDto>>> ExecuteAsync(Guid projectId, CancellationToken ct = default);
    }

    public class GetPublicProjectChannelsUseCase : IGetPublicProjectChannelsUseCase
    {
        private readonly IChannelRepository _channelRepository;
        private readonly IProjectRepository _projectRepository;

        public GetPublicProjectChannelsUseCase(IChannelRepository channelRepository, IProjectRepository projectRepository)
        {
            _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<Result<IEnumerable<ChannelPublicDto>>> ExecuteAsync(Guid projectId, CancellationToken ct = default)
        {
            var project = await _projectRepository.GetAsync(projectId, ct);
            if (project == null)
            {
                return Result<IEnumerable<ChannelPublicDto>>.Fail("Project not found");
            }

            var channels = await _channelRepository.GetByProjectIdAsync(projectId, ct);

            // Here we map the logical 'Credentials' field to the public 'Value' field.
            // Since the user defined Credentials as holding the "link or data", we expose it raw or processed.
            // Assuming it contains JSON, we pass it as string.
            
            var dtos = channels.Select(c => new ChannelPublicDto
            {
                Type = c.Type,
                Value = c.Credentials ?? "{}"
            }).ToList();

            return Result<IEnumerable<ChannelPublicDto>>.Ok(dtos);
        }
    }
}
