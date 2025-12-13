using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Auth;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Application.UseCases.Auth.ClientAuth.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Auth.ClientAuth
{
    public class ClientAuthUseCase : IClientAuthUseCase
    {
        private readonly IAppUserRepository _appUserRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAppUserPasswordResetTokenRepository _resetTokenRepo;
        private readonly IEmailService _emailService;

        public ClientAuthUseCase(
            IAppUserRepository appUserRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IAppUserPasswordResetTokenRepository resetTokenRepo,
            IEmailService emailService)
        {
            _appUserRepository = appUserRepository ?? throw new ArgumentNullException(nameof(appUserRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _resetTokenRepo = resetTokenRepo ?? throw new ArgumentNullException(nameof(resetTokenRepo));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<Result<ClientAuthResponse>> LoginAsync(ClientLoginRequest request, CancellationToken ct = default)
        {
            var user = await _appUserRepository.GetByEmailAsync(request.Email, ct);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                return Result<ClientAuthResponse>.Fail("Credenciales inválidas");
            }

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Result<ClientAuthResponse>.Fail("Credenciales inválidas");
            }

            var token = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, "Client");
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            return Result<ClientAuthResponse>.Ok(new ClientAuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                FullName = user.FullName,
                Email = user.Email
            });
        }

        public async Task<Result<ClientAuthResponse>> RegisterAsync(ClientRegisterRequest request, CancellationToken ct = default)
        {
            var existingUser = await _appUserRepository.GetByEmailAsync(request.Email, ct);
            if (existingUser != null)
            {
                if (!string.IsNullOrEmpty(existingUser.PasswordHash))
                {
                    return Result<ClientAuthResponse>.Fail("El usuario ya existe");
                }

                // Upgrade existing bot-user
                var hash = _passwordHasher.HashPassword(request.Password);
                existingUser.SetPassword(hash);
                existingUser.UpdateInfo(request.FullName, request.Phone ?? existingUser.Phone);
                
                await _appUserRepository.UpdateAsync(existingUser, ct);

                var token = _jwtTokenService.GenerateAccessToken(existingUser.Id, existingUser.Email, "Client");
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                return Result<ClientAuthResponse>.Ok(new ClientAuthResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    FullName = existingUser.FullName,
                    Email = existingUser.Email
                });
            }
            else
            {
                var newUser = new AppUser(request.Email, request.FullName, request.Phone, "email");
                var hash = _passwordHasher.HashPassword(request.Password);
                newUser.SetPassword(hash);

                await _appUserRepository.AddAsync(newUser, ct);

                // Send Welcome Email (Non-blocking ideally, but kept simple here)
                try {
                    await _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FullName);
                } catch { /* log error but don't fail registration */ }

                var token = _jwtTokenService.GenerateAccessToken(newUser.Id, newUser.Email, "Client");
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                return Result<ClientAuthResponse>.Ok(new ClientAuthResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    FullName = newUser.FullName,
                    Email = newUser.Email
                });
            }
        }

        public async Task<Result> ForgotPasswordAsync(ClientForgotPasswordRequest request, CancellationToken ct = default)
        {
            var user = await _appUserRepository.GetByEmailAsync(request.Email, ct);
            if (user == null)
            {
                // No revelar existencia
                return Result.Ok(); 
            }

            // Invalidate old tokens
            await _resetTokenRepo.InvalidateAllUserTokensAsync(user.Id, ct);

            // Create new token
            var tokenString = Guid.NewGuid().ToString("N");
            var resetToken = new AppUserPasswordResetToken(user.Id, tokenString);
            await _resetTokenRepo.AddAsync(resetToken, ct);

            // Send Email
            await _emailService.SendPasswordResetAsync(user.Email, user.FullName, tokenString);

            return Result.Ok();
        }

        public async Task<Result> ResetPasswordAsync(ClientResetPasswordRequest request, CancellationToken ct = default)
        {
            var tokenEntity = await _resetTokenRepo.GetByTokenAsync(request.Token, ct);
            if (tokenEntity == null || !tokenEntity.CanBeUsed())
            {
                return Result.Fail("Token inválido o expirado");
            }

            var user = await _appUserRepository.GetByIdAsync(tokenEntity.AppUserId, ct);
            if (user == null)
            {
                return Result.Fail("Usuario no encontrado");
            }

            var newHash = _passwordHasher.HashPassword(request.NewPassword);
            user.SetPassword(newHash);
            await _appUserRepository.UpdateAsync(user, ct);

            tokenEntity.MarkAsUsed();
            await _resetTokenRepo.UpdateAsync(tokenEntity, ct);

            return Result.Ok();
        }

        public async Task<Result<ClientProfileDto>> GetProfileAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _appUserRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                return Result<ClientProfileDto>.Fail("Usuario no encontrado");
            }

            return Result<ClientProfileDto>.Ok(new ClientProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified
            });
        }
    }
}
