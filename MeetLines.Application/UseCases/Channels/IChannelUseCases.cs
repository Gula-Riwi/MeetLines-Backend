using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Channels;

namespace MeetLines.Application.UseCases.Channels
{
    public interface ICreateChannelUseCase
    {
        Task<Result<ChannelDto>> ExecuteAsync(Guid userId, Guid projectId, CreateChannelRequest request, CancellationToken ct = default);
    }

    public interface IGetProjectChannelsUseCase
    {
        Task<Result<IEnumerable<ChannelDto>>> ExecuteAsync(Guid userId, Guid projectId, CancellationToken ct = default);
    }

    public interface IDeleteChannelUseCase
    {
        Task<Result<bool>> ExecuteAsync(Guid userId, Guid channelId, CancellationToken ct = default);
    }
}
