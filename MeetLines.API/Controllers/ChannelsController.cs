using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.Channels;
using MeetLines.Application.UseCases.Channels;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class ChannelsController : ControllerBase
    {
        private readonly ICreateChannelUseCase _createChannelUseCase;
        private readonly IGetProjectChannelsUseCase _getProjectChannelsUseCase;
        private readonly IDeleteChannelUseCase _deleteChannelUseCase;
        private readonly IGetPublicProjectChannelsUseCase _getPublicProjectChannelsUseCase;
        private readonly IUpdateChannelUseCase _updateChannelUseCase;

        public ChannelsController(
            ICreateChannelUseCase createChannelUseCase,
            IGetProjectChannelsUseCase getProjectChannelsUseCase,
            IDeleteChannelUseCase deleteChannelUseCase,
            IGetPublicProjectChannelsUseCase getPublicProjectChannelsUseCase,
            IUpdateChannelUseCase updateChannelUseCase)
        {
            _createChannelUseCase = createChannelUseCase ?? throw new ArgumentNullException(nameof(createChannelUseCase));
            _getProjectChannelsUseCase = getProjectChannelsUseCase ?? throw new ArgumentNullException(nameof(getProjectChannelsUseCase));
            _deleteChannelUseCase = deleteChannelUseCase ?? throw new ArgumentNullException(nameof(deleteChannelUseCase));
            _getPublicProjectChannelsUseCase = getPublicProjectChannelsUseCase ?? throw new ArgumentNullException(nameof(getPublicProjectChannelsUseCase));
            _updateChannelUseCase = updateChannelUseCase ?? throw new ArgumentNullException(nameof(updateChannelUseCase));
        }

        [HttpPost("projects/{projectId}/channels")]
        public async Task<IActionResult> CreateChannel(Guid projectId, [FromBody] CreateChannelRequest request, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _createChannelUseCase.ExecuteAsync(userId, projectId, request, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(CreateChannel), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("channels/{channelId}")]
        public async Task<IActionResult> UpdateChannel(Guid channelId, [FromBody] UpdateChannelRequest request, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _updateChannelUseCase.ExecuteAsync(userId, channelId, request, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("projects/{projectId}/channels")]
        public async Task<IActionResult> GetProjectChannels(Guid projectId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _getProjectChannelsUseCase.ExecuteAsync(userId, projectId, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("projects/{projectId}/channels/public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProjectChannels(Guid projectId, CancellationToken ct)
        {
            var result = await _getPublicProjectChannelsUseCase.ExecuteAsync(projectId, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpDelete("channels/{channelId}")]
        public async Task<IActionResult> DeleteChannel(Guid channelId, CancellationToken ct)
        {
            var userId = GetUserId();
            var result = await _deleteChannelUseCase.ExecuteAsync(userId, channelId, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID in token");
            return userId;
        }
    }
}
