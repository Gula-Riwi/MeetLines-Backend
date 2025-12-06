using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Profile;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ISaasUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILoginSessionRepository _loginSessionRepository;
        private readonly IEmailService _emailService;
        private readonly DiscordWebhookService _discordService; // <--- Nuevo servicio

        public ProfileService(
            ISaasUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ILoginSessionRepository loginSessionRepository,
            IEmailService emailService,
            DiscordWebhookService discordService) // <--- Inyección
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _loginSessionRepository = loginSessionRepository ?? throw new ArgumentNullException(nameof(loginSessionRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _discordService = discordService; // <--- Asignación
        }

        public async Task<Result<GetProfileResponse>> GetProfileAsync(Guid userId, CancellationToken ct = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, ct);
                if (user == null)
                {
                    return Result<GetProfileResponse>.Fail("Usuario no encontrado");
                }

                var response = new GetProfileResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Timezone = user.Timezone,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    IsEmailVerified = user.IsEmailVerified,
                    AuthProvider = user.AuthProvider,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                };

                return Result<GetProfileResponse>.Ok(response);
            }
            catch (Exception ex)
            {
                return Result<GetProfileResponse>.Fail($"Error al obtener perfil: {ex.Message}");
            }
        }

        public async Task<Result<GetProfileResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, ct);
                if (user == null)
                {
                    return Result<GetProfileResponse>.Fail("Usuario no encontrado");
                }

                // Actualizar perfil
                user.UpdateProfile(request.Name, request.Phone, request.Timezone);
                await _userRepository.UpdateAsync(user, ct);

                // [DISCORD] Notificar actualización
                try
                {
                    string changes = $"Nombre: {request.Name}, Tel: {request.Phone ?? "N/A"}, Zona: {request.Timezone}";
                    await _discordService.SendProfileUpdatedAsync(user.Name, user.Email, changes);
                }
                catch { /* Ignorar error de log */ }

                var response = new GetProfileResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Timezone = user.Timezone,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    IsEmailVerified = user.IsEmailVerified,
                    AuthProvider = user.AuthProvider,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                };

                return Result<GetProfileResponse>.Ok(response);
            }
            catch (Exception ex)
            {
                return Result<GetProfileResponse>.Fail($"Error al actualizar perfil: {ex.Message}");
            }
        }

        public async Task<Result<GetProfileResponse>> UpdateProfilePictureAsync(Guid userId, UpdateProfilePictureRequest request, CancellationToken ct = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, ct);
                if (user == null)
                {
                    return Result<GetProfileResponse>.Fail("Usuario no encontrado");
                }

                // Actualizar foto de perfil
                user.UpdateProfilePicture(request.ProfilePictureUrl);
                await _userRepository.UpdateAsync(user, ct);

                // [DISCORD] Notificar cambio de foto
                try
                {
                    await _discordService.SendProfileUpdatedAsync(user.Name, user.Email, "Foto de perfil actualizada");
                }
                catch { }

                var response = new GetProfileResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Timezone = user.Timezone,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    IsEmailVerified = user.IsEmailVerified,
                    AuthProvider = user.AuthProvider,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                };

                return Result<GetProfileResponse>.Ok(response);
            }
            catch (Exception ex)
            {
                return Result<GetProfileResponse>.Fail($"Error al actualizar foto de perfil: {ex.Message}");
            }
        }

        public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, ct);
                if (user == null)
                {
                    return Result.Fail("Usuario no encontrado");
                }

                // Verificar que el usuario use autenticación local
                if (!user.RequiresPassword())
                {
                    return Result.Fail("No puedes cambiar la contraseña porque usas autenticación OAuth");
                }

                // Verificar contraseña actual
                if (user.PasswordHash == null || !_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    return Result.Fail("La contraseña actual es incorrecta");
                }

                // Cambiar contraseña
                var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                user.ChangePassword(newPasswordHash);
                await _userRepository.UpdateAsync(user, ct);

                // Cerrar todas las sesiones activas por seguridad
                await _loginSessionRepository.DeleteAllUserSessionsAsync(userId, ct);
                
                // Enviar email
                try
                {
                    await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.Name);
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"⚠️ Error al enviar email de notificación: {emailEx.Message}");
                }

                // [DISCORD] Notificar cambio de seguridad crítico
                try
                {
                    await _discordService.SendPasswordChangedAsync(user.Name, user.Email);
                }
                catch { }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al cambiar contraseña: {ex.Message}");
            }
        }

        public async Task<Result> DeleteProfilePictureAsync(Guid userId, CancellationToken ct = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, ct);
                if (user == null)
                {
                    return Result.Fail("Usuario no encontrado");
                }

                // Eliminar foto de perfil (establecer como null)
                user.UpdateProfilePicture(null!);
                await _userRepository.UpdateAsync(user, ct);

                // [DISCORD] Notificar eliminación
                try
                {
                    await _discordService.SendProfileUpdatedAsync(user.Name, user.Email, "Foto de perfil eliminada");
                }
                catch { }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al eliminar foto de perfil: {ex.Message}");
            }
        }
    }
}