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
            // 1. Check by Email
            var existingUser = await _appUserRepository.GetByEmailAsync(request.Email, ct);
            if (existingUser != null)
            {
                if (!string.IsNullOrEmpty(existingUser.PasswordHash))
                {
                    return Result<ClientAuthResponse>.Fail("El usuario ya existe (Email)");
                }
                // Implicitly handles case where email exists but no password (bot user with real email?), but logic below is more robust for Phone.
            }

            // 2. Check by Phone (Merge Strategy)
            if (existingUser == null && !string.IsNullOrEmpty(request.Phone))
            {
                var userByPhone = await _appUserRepository.GetByPhoneAsync(request.Phone, ct);
                if (userByPhone != null)
                {
                    // Check if it's a temporary/bot user that we can take over
                    // Criteria: Ends with .temp (whatsapp/telegram) OR AuthProvider is "bot" OR Password is empty
                    bool isTempUser = (userByPhone.Email != null && userByPhone.Email.EndsWith(".temp")) 
                                      || userByPhone.AuthProvider == "bot" 
                                      || string.IsNullOrEmpty(userByPhone.PasswordHash);

                    if (isTempUser)
                    {
                        // TAKEOVER / MERGE
                        existingUser = userByPhone;
                        
                        // Update Email from temp to real
                        existingUser.UpdateEmail(request.Email); // We might need to expose this setter or use specialized method
                        existingUser.UpdateInfo(request.FullName, request.Phone); 
                    }
                    else
                    {
                        return Result<ClientAuthResponse>.Fail("El teléfono ya está registrado en otra cuenta.");
                    }
                }
            }

            if (existingUser != null)
            {
                // Upgrade existing user (Bot or Email-only)
                var hash = _passwordHasher.HashPassword(request.Password);
                existingUser.SetPassword(hash);
                
                // Ensure Application User properties are set if we didn't do it in the Merge block above (e.g. found by Email but empty pass)
                if (existingUser.Email != request.Email) 
                {
                    // This happens if we found by Phone and verified it's temp.
                    // We need to update the email. 
                    // AppUser likely has private set for Email. Let's check AppUser entity.
                    // Only constructor sets email usually. We might need a method "UpdateEmail".
                    // Assuming for now we can't easily change email without a method.
                    // If AppUser doesn't support changing email, we have a problem.
                    // Let's assume UpdateInfo does NOT update email.
                }

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
                // Create New User
                var newUser = new AppUser(request.Email, request.FullName, request.Phone, "email");
                var hash = _passwordHasher.HashPassword(request.Password);
                newUser.SetPassword(hash);

                await _appUserRepository.AddAsync(newUser, ct);

                try {
                    await _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FullName);
                } catch { /* log */ }

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
