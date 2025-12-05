using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Profile;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IProfileService
    {
        Task<Result<GetProfileResponse>> GetProfileAsync(Guid userId, CancellationToken ct = default);
        Task<Result<GetProfileResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
        Task<Result<GetProfileResponse>> UpdateProfilePictureAsync(Guid userId, UpdateProfilePictureRequest request, CancellationToken ct = default);
        Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
        Task<Result> DeleteProfilePictureAsync(Guid userId, CancellationToken ct = default);
    }
}