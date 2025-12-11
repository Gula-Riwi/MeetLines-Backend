using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Channels;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Channels
{
    public class CreateChannelUseCase : ICreateChannelUseCase
    {
        private readonly IChannelRepository _channelRepository;
        private readonly IProjectRepository _projectRepository;

        public CreateChannelUseCase(IChannelRepository channelRepository, IProjectRepository projectRepository)
        {
            _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<Result<ChannelDto>> ExecuteAsync(Guid userId, Guid projectId, CreateChannelRequest request, CancellationToken ct = default)
        {
            var project = await _projectRepository.GetAsync(projectId, ct);
            if (project == null)
                return Result<ChannelDto>.Fail("Project not found");

            // Verify ownership
            if (project.UserId != userId)
                return Result<ChannelDto>.Fail("Unauthorized access to project");

            var channel = new Channel(projectId, request.Type, request.Credentials);
            channel.Verify(); // Auto-verify on creation per user request
            
            await _channelRepository.CreateAsync(channel, ct);

            return Result<ChannelDto>.Ok(new ChannelDto
            {
                Id = channel.Id,
                ProjectId = channel.ProjectId,
                Type = channel.Type,
                Verified = channel.Verified,
                CreatedAt = channel.CreatedAt,
                Credentials = channel.Credentials
            });
        }
    }
}
