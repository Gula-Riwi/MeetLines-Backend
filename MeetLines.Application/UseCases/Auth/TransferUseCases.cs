using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Auth;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;
using MeetLines.Domain.Entities;

namespace MeetLines.Application.UseCases.Auth
{
    public class TransferUseCases : ITransferUseCases
    {
        private readonly ITransferTokenRepository _transferRepo;
        private readonly ISaasUserRepository _userRepository;
        private readonly ILoginSessionRepository _loginSessionRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public TransferUseCases(
            ITransferTokenRepository transferRepo,
            ISaasUserRepository userRepository,
            ILoginSessionRepository loginSessionRepository,
            IJwtTokenService jwtTokenService)
        {
            _transferRepo = transferRepo ?? throw new ArgumentNullException(nameof(transferRepo));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _loginSessionRepository = loginSessionRepository ?? throw new ArgumentNullException(nameof(loginSessionRepository));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        }

        public async Task<Result<string>> CreateTransferAsync(CreateTransferRequest request, string userId, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Tenant))
                    return Result<string>.Fail("Tenant is required");

                if (!Guid.TryParse(userId, out var uid))
                    return Result<string>.Fail("Invalid user id");

                // generate secure token
                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                var expiresAt = DateTimeOffset.UtcNow.AddMinutes(2);

                var t = new TransferToken(uid, token, request.Tenant, expiresAt);
                await _transferRepo.AddAsync(t, ct);

                return Result<string>.Ok(token);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail($"Error creating transfer token: {ex.Message}");
            }
        }

        public async Task<Result<LoginResponse>> AcceptTransferAsync(AcceptTransferRequest request, CancellationToken ct = default)
        {
            try
            {
                var token = request.TransferToken;
                var record = await _transferRepo.GetByTokenAsync(token, ct);
                if (record == null) return Result<LoginResponse>.Fail("Transfer token not found");
                if (record.Used) return Result<LoginResponse>.Fail("Transfer token already used");
                if (record.IsExpired()) return Result<LoginResponse>.Fail("Transfer token expired");

                var user = await _userRepository.GetByIdAsync(record.UserId, ct);
                if (user == null) return Result<LoginResponse>.Fail("User not found");

                // create login session and tokens similar to OAuthLoginUseCase
                var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, "User");
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var tokenHash = _jwtTokenService.ComputeTokenHash(refreshToken);

                var session = new LoginSession(
                    user.Id,
                    tokenHash,
                    null,
                    null,
                    DateTimeOffset.UtcNow.AddDays(7)
                );
                await _loginSessionRepository.AddAsync(session, ct);

                // mark transfer token used
                record.MarkUsed();
                await _transferRepo.UpdateAsync(record, ct);

                user.UpdateLastLogin();
                await _userRepository.UpdateAsync(user, ct);

                return Result<LoginResponse>.Ok(new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    IsEmailVerified = user.IsEmailVerified,
                    ExpiresAt = session.ExpiresAt!.Value
                });
            }
            catch (Exception ex)
            {
                return Result<LoginResponse>.Fail($"Error accepting transfer: {ex.Message}");
            }
        }
    }
}
