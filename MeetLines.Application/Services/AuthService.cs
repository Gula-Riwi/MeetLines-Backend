using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Auth;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ISaasUserRepository _userRepository;
        private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly ILoginSessionRepository _loginSessionRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public AuthService(
            ISaasUserRepository userRepository,
            IEmailVerificationTokenRepository emailVerificationTokenRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            ILoginSessionRepository loginSessionRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            ISubscriptionRepository subscriptionRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailVerificationTokenRepository = emailVerificationTokenRepository ?? throw new ArgumentNullException(nameof(emailVerificationTokenRepository));
            _passwordResetTokenRepository = passwordResetTokenRepository ?? throw new ArgumentNullException(nameof(passwordResetTokenRepository));
            _loginSessionRepository = loginSessionRepository ?? throw new ArgumentNullException(nameof(loginSessionRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        }

        public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            try
            {
                // Verificar si el email ya existe
                var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
                if (existingUser != null)
                {
                    return Result<RegisterResponse>.Fail("El email ya está registrado");
                }

                // Crear el usuario
                var passwordHash = _passwordHasher.HashPassword(request.Password);
                var user = SaasUser.CreateLocalUser(request.Name, request.Email, passwordHash, request.Timezone);
                
                if (!string.IsNullOrEmpty(request.Phone))
                {
                    user.UpdateProfile(request.Name, request.Phone, request.Timezone);
                }

                await _userRepository.AddAsync(user, ct);
                
                // ===== CREAR SUSCRIPCIÓN GRATUITA =====
                var freeSubscription = new Subscription(
                    userId: user.Id,
                    plan: "beginner",
                    cycle: "monthly",
                    price: 0m
                );
                await _subscriptionRepository.AddAsync(freeSubscription, ct);

                // Crear token de verificación de email
                var verificationToken = Guid.NewGuid().ToString("N");
                var emailToken = new EmailVerificationToken(user.Id, verificationToken);
                await _emailVerificationTokenRepository.AddAsync(emailToken, ct);

                // Enviar email de verificación
                await _emailService.SendEmailVerificationAsync(user.Email, user.Name, verificationToken);

                return Result<RegisterResponse>.Ok(new RegisterResponse
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Message = "Registro exitoso. Por favor verifica tu email.",
                    Plan = "beginner"
                });
            }
            catch (Exception ex)
            {
                return Result<RegisterResponse>.Fail($"Error en el registro: {ex.Message}");
            }
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            try
            {
                // Buscar usuario por email
                var user = await _userRepository.GetByEmailAsync(request.Email, ct);
                if (user == null)
                {
                    return Result<LoginResponse>.Fail("Email o contraseña incorrectos");
                }

                // Verificar si el usuario puede hacer login
                if (!user.CanLogin())
                {
                    return Result<LoginResponse>.Fail("Usuario desactivado");
                }

                // Verificar contraseña
                if (!user.RequiresPassword() || user.PasswordHash == null)
                {
                    return Result<LoginResponse>.Fail("Este usuario debe iniciar sesión con OAuth");
                }

                if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                {
                    return Result<LoginResponse>.Fail("Email o contraseña incorrectos");
                }

                // Generar tokens
                var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, "User");
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var tokenHash = _jwtTokenService.ComputeTokenHash(refreshToken);

                // Guardar sesión
                var session = new LoginSession(
                    user.Id,
                    tokenHash,
                    request.DeviceInfo,
                    request.IpAddress,
                    DateTimeOffset.UtcNow.AddDays(7)
                );
                await _loginSessionRepository.AddAsync(session, ct);

                // Actualizar último login
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
                return Result<LoginResponse>.Fail($"Error en el login: {ex.Message}");
            }
        }

        public async Task<Result<LoginResponse>> OAuthLoginAsync(OAuthLoginRequest request, CancellationToken ct = default)
        {
            try
            {
                // Buscar usuario por external provider ID
                var user = await _userRepository.GetByExternalProviderIdAsync(request.ExternalProviderId, ct);

                // Si no existe, buscar por email
                if (user == null)
                {
                    user = await _userRepository.GetByEmailAsync(request.Email, ct);
                }

                // Si no existe, crear nuevo usuario
                if (user == null)
                {
                    user = SaasUser.CreateOAuthUser(
                        request.Name,
                        request.Email,
                        request.Provider,
                        request.ExternalProviderId,
                        request.ProfilePictureUrl
                    );
                    await _userRepository.AddAsync(user, ct);
                    
                    // ===== CREAR SUSCRIPCIÓN GRATUITA PARA OAUTH
                    
                    var freeSubscription = new Subscription(
                        userId: user.Id,
                        plan: "beginner",
                        cycle: "monthly",
                        price: 0m
                    );
                    await _subscriptionRepository.AddAsync(freeSubscription, ct);
                    
                    // Enviar email de bienvenida
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);
                }

                // Verificar si el usuario puede hacer login
                if (!user.CanLogin())
                {
                    return Result<LoginResponse>.Fail("Usuario desactivado");
                }

                // Generar tokens
                var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, "User");
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var tokenHash = _jwtTokenService.ComputeTokenHash(refreshToken);

                // Guardar sesión
                var session = new LoginSession(
                    user.Id,
                    tokenHash,
                    request.DeviceInfo,
                    request.IpAddress,
                    DateTimeOffset.UtcNow.AddDays(7)
                );
                await _loginSessionRepository.AddAsync(session, ct);

                // Actualizar último login
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
                return Result<LoginResponse>.Fail($"Error en OAuth login: {ex.Message}");
            }
        }

        public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
        {
            try
            {
                var tokenHash = _jwtTokenService.ComputeTokenHash(request.RefreshToken);
                var session = await _loginSessionRepository.GetByTokenHashAsync(tokenHash, ct);

                if (session == null || session.IsExpired())
                {
                    return Result<LoginResponse>.Fail("Refresh token inválido o expirado");
                }

                var user = await _userRepository.GetByIdAsync(session.UserId, ct);
                if (user == null || !user.CanLogin())
                {
                    return Result<LoginResponse>.Fail("Usuario no encontrado o desactivado");
                }

                // Generar nuevos tokens
                var newAccessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, "User");
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
                var newTokenHash = _jwtTokenService.ComputeTokenHash(newRefreshToken);

                // Eliminar sesión anterior y crear nueva
                await _loginSessionRepository.DeleteAsync(session.Id, ct);
                
                var newSession = new LoginSession(
                    user.Id,
                    newTokenHash,
                    request.DeviceInfo ?? session.DeviceInfo,
                    request.IpAddress ?? session.IpAddress,
                    DateTimeOffset.UtcNow.AddDays(7)
                );
                await _loginSessionRepository.AddAsync(newSession, ct);

                return Result<LoginResponse>.Ok(new LoginResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    UserId = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    IsEmailVerified = user.IsEmailVerified,
                    ExpiresAt = newSession.ExpiresAt!.Value
                });
            }
            catch (Exception ex)
            {
                return Result<LoginResponse>.Fail($"Error al refrescar token: {ex.Message}");
            }
        }

        public async Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
        {
            try
            {
                var token = await _emailVerificationTokenRepository.GetByTokenAsync(request.Token, ct);
                
                if (token == null || !token.CanBeUsed())
                {
                    return Result.Fail("Token inválido o expirado");
                }

                var user = await _userRepository.GetByIdAsync(token.UserId, ct);
                if (user == null)
                {
                    return Result.Fail("Usuario no encontrado");
                }

                // Verificar email
                user.VerifyEmail();
                await _userRepository.UpdateAsync(user, ct);

                // Marcar token como usado
                token.MarkAsVerified();
                await _emailVerificationTokenRepository.UpdateAsync(token, ct);

                // Enviar email de bienvenida
                await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al verificar email: {ex.Message}");
            }
        }

        public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(request.Email, ct);
                if (user == null)
                {
                    // Por seguridad, no revelar si el email existe o no
                    return Result.Ok();
                }

                // Verificar que sea usuario local
                if (!user.RequiresPassword())
                {
                    return Result.Fail("Este usuario utiliza OAuth y no tiene contraseña");
                }

                // Invalidar tokens anteriores
                await _passwordResetTokenRepository.InvalidateAllUserTokensAsync(user.Id, ct);

                // Crear nuevo token
                var resetToken = Guid.NewGuid().ToString("N");
                var token = new PasswordResetToken(user.Id, resetToken, 1, request.IpAddress);
                await _passwordResetTokenRepository.AddAsync(token, ct);

                // Enviar email
                await _emailService.SendPasswordResetAsync(user.Email, user.Name, resetToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al solicitar recuperación: {ex.Message}");
            }
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        {
            try
            {
                var token = await _passwordResetTokenRepository.GetByTokenAsync(request.Token, ct);
                
                if (token == null || !token.CanBeUsed())
                {
                    return Result.Fail("Token inválido o expirado");
                }

                var user = await _userRepository.GetByIdAsync(token.UserId, ct);
                if (user == null)
                {
                    return Result.Fail("Usuario no encontrado");
                }

                // Cambiar contraseña
                var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                user.ChangePassword(newPasswordHash);
                await _userRepository.UpdateAsync(user, ct);

                // Marcar token como usado
                token.MarkAsUsed();
                await _passwordResetTokenRepository.UpdateAsync(token, ct);

                // Cerrar todas las sesiones activas por seguridad
                await _loginSessionRepository.DeleteAllUserSessionsAsync(user.Id, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al restablecer contraseña: {ex.Message}");
            }
        }

        public async Task<Result> ResendVerificationEmailAsync(string email, CancellationToken ct = default)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email, ct);
                if (user == null)
                {
                    return Result.Fail("Usuario no encontrado");
                }

                if (user.IsEmailVerified)
                {
                    return Result.Fail("El email ya está verificado");
                }

                // Invalidar tokens anteriores
                await _emailVerificationTokenRepository.InvalidateAllUserTokensAsync(user.Id, ct);

                // Crear nuevo token
                var verificationToken = Guid.NewGuid().ToString("N");
                var token = new EmailVerificationToken(user.Id, verificationToken);
                await _emailVerificationTokenRepository.AddAsync(token, ct);

                // Enviar email
                await _emailService.SendEmailVerificationAsync(user.Email, user.Name, verificationToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al reenviar verificación: {ex.Message}");
            }
        }

        public async Task<Result> LogoutAsync(string refreshToken, CancellationToken ct = default)
        {
            try
            {
                var tokenHash = _jwtTokenService.ComputeTokenHash(refreshToken);
                var session = await _loginSessionRepository.GetByTokenHashAsync(tokenHash, ct);

                if (session != null)
                {
                    await _loginSessionRepository.DeleteAsync(session.Id, ct);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al cerrar sesión: {ex.Message}");
            }
        }
    }
}