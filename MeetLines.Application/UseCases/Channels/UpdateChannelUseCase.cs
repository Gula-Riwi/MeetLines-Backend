using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Channels;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Channels
{
    public interface IUpdateChannelUseCase
    {
        Task<Result<ChannelDto>> ExecuteAsync(Guid userId, Guid channelId, UpdateChannelRequest request, CancellationToken ct = default);
    }

    public class UpdateChannelUseCase : IUpdateChannelUseCase
    {
        private readonly IChannelRepository _channelRepository;
        private readonly IProjectRepository _projectRepository;

        public UpdateChannelUseCase(IChannelRepository channelRepository, IProjectRepository projectRepository)
        {
            _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<Result<ChannelDto>> ExecuteAsync(Guid userId, Guid channelId, UpdateChannelRequest request, CancellationToken ct = default)
        {
            var channel = await _channelRepository.GetByIdAsync(channelId, ct);
            if (channel == null) return Result<ChannelDto>.Fail("Channel not found");

            // Verify project access
            var project = await _projectRepository.GetAsync(channel.ProjectId, ct);
            if (project == null || project.UserId != userId)
                return Result<ChannelDto>.Fail("Unauthorized");

            if (request.Credentials != null)
            {
                channel.UpdateCredentials(request.Credentials);
            }

            await _channelRepository.UpdateAsync(channel, ct);

            return Result<ChannelDto>.Ok(new ChannelDto
            {
                Id = channel.Id,
                ProjectId = channel.ProjectId,
                Type = channel.Type,
                Verified = channel.Verified,
                CreatedAt = channel.CreatedAt
            });
        }
    }
}
