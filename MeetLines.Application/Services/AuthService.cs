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
        private readonly IDiscordWebhookService _discordService;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeePasswordResetTokenRepository _employeePasswordResetTokenRepository;
        private readonly ITenantService _tenantService;
        private readonly MeetLines.Application.Services.Interfaces.ITransferUseCases _transferUseCases;

        public AuthService(
            ISaasUserRepository userRepository,
            IEmailVerificationTokenRepository emailVerificationTokenRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            ILoginSessionRepository loginSessionRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            ISubscriptionRepository subscriptionRepository,
            IDiscordWebhookService discordService,
            IEmployeeRepository employeeRepository,
            IEmployeePasswordResetTokenRepository employeePasswordResetTokenRepository,
            ITenantService tenantService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailVerificationTokenRepository = emailVerificationTokenRepository ?? throw new ArgumentNullException(nameof(emailVerificationTokenRepository));
            _passwordResetTokenRepository = passwordResetTokenRepository ?? throw new ArgumentNullException(nameof(passwordResetTokenRepository));
            _loginSessionRepository = loginSessionRepository ?? throw new ArgumentNullException(nameof(loginSessionRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
            _discordService = discordService;
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _employeePasswordResetTokenRepository = employeePasswordResetTokenRepository ?? throw new ArgumentNullException(nameof(employeePasswordResetTokenRepository));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _transferUseCases = null!; // will be injected via DI by ApplicationServiceCollectionExtensions
        }

        // NOTE: Transfer methods are delegated to ITransferUseCases to keep responsibilities separated
        public async Task<Result<string>> CreateTransferAsync(CreateTransferRequest request, string userId, CancellationToken ct = default)
        {
            if (_transferUseCases == null) return Result<string>.Fail("Transfer service not available");
            return await _transferUseCases.CreateTransferAsync(request, userId, ct);
        }

        public async Task<Result<LoginResponse>> AcceptTransferAsync(AcceptTransferRequest request, CancellationToken ct = default)
        {
            if (_transferUseCases == null) return Result<LoginResponse>.Fail("Transfer service not available");
            return await _transferUseCases.AcceptTransferAsync(request, ct);
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

                // [DISCORD] Notificar nuevo registro
                try 
                {
                    await _discordService.SendEmbedAsync(
                        "🎉 Nuevo Usuario Registrado", 
                        $"**Nombre:** {user.Name}\n**Email:** {user.Email}\n**Plan:** Beginner (Free)", 
                        5763719); // Verde
                }
                catch { /* Ignorar error de log para no bloquear el registro */ }

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

        public async Task<Result<LoginResponse>> EmployeeLoginAsync(EmployeeLoginRequest request, CancellationToken ct = default)
        {
            try
            {
                var employee = await _employeeRepository.GetByUsernameAsync(request.Username, ct);
                if (employee == null)
                {
                    return Result<LoginResponse>.Fail("Usuario o contraseña incorrectos");
                }

                // ✅ VALIDACIÓN: Verificar que el empleado pertenezca al tenant actual
                var currentTenantId = _tenantService.GetCurrentTenantId();
                // Solo validar si HAY un tenant resuelto (si es null, es dominio principal o reservado, permitir acceso o decidir política)
                // En este caso, permitimos login desde dominio principal si se conocen las credenciales
                if (currentTenantId.HasValue && currentTenantId.Value != Guid.Empty && employee.ProjectId != currentTenantId.Value)
                {
                    // El empleado no pertenece a este tenant/proyecto
                    return Result<LoginResponse>.Fail("Usuario o contraseña incorrectos");
                }

                if (!employee.IsActive)
                {
                    return Result<LoginResponse>.Fail("Cuenta desactivada");
                }

                if (!_passwordHasher.VerifyPassword(request.Password, employee.PasswordHash))
                {
                    return Result<LoginResponse>.Fail("Usuario o contraseña incorrectos");
                }

                // Generar tokens para Empleado
                // Usamos un rol especial o claims adicionales para identificar que es un empleado y de qué proyecto
                var accessToken = _jwtTokenService.GenerateAccessToken(employee.Id, employee.Username, employee.Role); 
                // Nota: GenerateAccessToken por defecto usa "User" como rol en la implementación actual de AuthService para SaasUser,
                // pero aquí pasamos employee.Role.
                
                // NOTA IMPORTANTE: El método GenerateAccessToken actual podría no aceptar claims custom como ProjectId.
                // Idealmente deberíamos extender JwtTokenService, pero por ahora usaremos el token estándar.
                // El frontend deberá decodificar el token o usar un endpoint de Profile para saber el ProjectId si no va en los claims standard.
                // O mejor, el LoginResponse puede devolver esa info.

                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var tokenHash = _jwtTokenService.ComputeTokenHash(refreshToken);

                var session = new LoginSession(
                    employee.Id,
                    tokenHash,
                    request.DeviceInfo,
                    request.IpAddress,
                    DateTimeOffset.UtcNow.AddDays(7)
                );
                await _loginSessionRepository.AddAsync(session, ct);

                return Result<LoginResponse>.Ok(new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    UserId = employee.Id,
                    Email = employee.Username, // Reuse Email field for username
                    Name = employee.Name,
                    IsEmailVerified = true,
                    ExpiresAt = session.ExpiresAt!.Value
                });
            }
            catch (Exception ex)
            {
                return Result<LoginResponse>.Fail($"Error en login de empleado: {ex.Message}");
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
                    // [DISCORD] Opcional: Notificar intento fallido (útil para seguridad)
                    // await _discordService.SendInfoAsync("⚠️ Login Fallido", $"Intento fallido para {request.Email}");
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

                // Actualizar último login
                user.UpdateLastLogin();
                await _userRepository.UpdateAsync(user, ct);

                // [DISCORD] Login notification removed to reduce noise


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
                // Si no tenemos email, generar uno basado en el provider y ID
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    request.Email = $"{request.Provider.ToString().ToLower()}_{request.ExternalProviderId}@oauth.meetlines.local";
                }

                // Buscar usuario por external provider ID
                var user = await _userRepository.GetByExternalProviderIdAsync(request.ExternalProviderId, ct);
                bool isNewUser = false;

                // Si no existe, buscar por email
                if (user == null && !string.IsNullOrWhiteSpace(request.Email))
                {
                    user = await _userRepository.GetByEmailAsync(request.Email, ct);
                }

                // Si no existe, crear nuevo usuario
                if (user == null)
                {
                    isNewUser = true;
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

                    // [DISCORD] Notificar registro via OAuth
                    try
                    {
                        await _discordService.SendEmbedAsync(
                            "🌐 Nuevo Registro (OAuth)", 
                            $"Usuario: {user.Name}\nEmail: {user.Email}\nProveedor: {request.Provider}", 
                            5763719);
                    }
                    catch { }
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

                // [DISCORD] Login notification removed to reduce noise

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

                // Enviar email de bienvenida (Originalmente era aquí, pero el requisito es "Verified Correctly")
                // Cambiaremos esto para usar el nuevo método específico
                await _emailService.SendEmailVerifiedNotificationAsync(user.Email, user.Name);

                // [DISCORD] Notification removed to reduce noise

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
                
                // [DISCORD] Notification removed to reduce noise

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

                // [DISCORD] Notificación de cambio de contraseña
                try
                {
                    await _discordService.SendEmbedAsync("🔄 Contraseña Cambiada", $"El usuario **{user.Email}** ha cambiado su contraseña exitosamente.", 16776960); // Amarillo
                }
                catch { }

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
                    
                    // [DISCORD] Opcional: Log de salida
                    // try { await _discordService.SendInfoAsync("👋 Logout", $"Usuario ID: {session.UserId}"); } catch {}
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al cerrar sesión: {ex.Message}");
            }
        }
        public async Task<Result> EmployeeForgotPasswordAsync(EmployeeForgotPasswordRequest request, CancellationToken ct = default)
        {
            try
            {
                // Buscamos empleado por Email explícitamente para recuperación
                var employee = await _employeeRepository.GetByEmailAsync(request.Email, ct);
                if (employee == null)
                {
                    // Por seguridad, no revelar si existe
                    return Result.Ok();
                }

                // Invalidar tokens anteriores
                await _employeePasswordResetTokenRepository.InvalidateAllUserTokensAsync(employee.Id, ct);

                // Crear nuevo token
                var resetToken = Guid.NewGuid().ToString("N");
                var token = new EmployeePasswordResetToken(employee.Id, resetToken);
                await _employeePasswordResetTokenRepository.AddAsync(token, ct);

                // Enviar email de recuperación
                // Usamos el mismo método de email service, asumiendo que el texto es genérico o agregamos uno específico si se requiere
                await _emailService.SendPasswordResetAsync(employee.Email, employee.Name, resetToken);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al solicitar recuperación de empleado: {ex.Message}");
            }
        }

        public async Task<Result> EmployeeResetPasswordAsync(EmployeeResetPasswordRequest request, CancellationToken ct = default)
        {
            try
            {
                var token = await _employeePasswordResetTokenRepository.GetByTokenAsync(request.Token, ct);

                if (token == null || !token.CanBeUsed())
                {
                    return Result.Fail("Token inválido o expirado");
                }

                var employee = await _employeeRepository.GetByIdAsync(token.EmployeeId, ct);
                if (employee == null)
                {
                    return Result.Fail("Empleado no encontrado");
                }

                // Cambiar contraseña
                var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                employee.ChangePassword(newPasswordHash);
                await _employeeRepository.UpdateAsync(employee, ct);

                // Marcar token como usado
                token.MarkAsUsed();
                await _employeePasswordResetTokenRepository.UpdateAsync(token, ct);
                
                // Opción: invalidar sesiones de login del empleado si existiera tabla de sesiones separada o compartida
                // Por ahora no invalidamos sesiones activas de empleado explícitamente salvo que LoginSession soporte empleados (que sí lo hace mediante UserId)
                // await _loginSessionRepository.DeleteAllUserSessionsAsync(employee.Id, ct); 

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al restablecer contraseña de empleado: {ex.Message}");
            }
        }

        public async Task<Result> EmployeeChangePasswordAsync(Guid employeeId, EmployeeChangePasswordRequest request, CancellationToken ct = default)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId, ct);
                if (employee == null)
                {
                    return Result.Fail("Empleado no encontrado");
                }

                // Verificar contraseña actual
                if (!_passwordHasher.VerifyPassword(request.CurrentPassword, employee.PasswordHash))
                {
                    return Result.Fail("La contraseña actual es incorrecta");
                }

                // Cambiar contraseña
                var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                employee.ChangePassword(newPasswordHash);
                await _employeeRepository.UpdateAsync(employee, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al cambiar contraseña: {ex.Message}");
            }
        }
    }
}