using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Channels
{
    public class DeleteChannelUseCase : IDeleteChannelUseCase
    {
        private readonly IChannelRepository _channelRepository;
        private readonly IProjectRepository _projectRepository; // To check ownership via project

        public DeleteChannelUseCase(IChannelRepository channelRepository, IProjectRepository projectRepository)
        {
            _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<Result<bool>> ExecuteAsync(Guid userId, Guid channelId, CancellationToken ct = default)
        {
            var channel = await _channelRepository.GetByIdAsync(channelId, ct);
            if (channel == null) return Result<bool>.Fail("Channel not found");

            var project = await _projectRepository.GetAsync(channel.ProjectId, ct);
            if (project == null || project.UserId != userId)
                return Result<bool>.Fail("Unauthorized");

            await _channelRepository.DeleteAsync(channel, ct);
            return Result<bool>.Ok(true);
        }
    }
}
