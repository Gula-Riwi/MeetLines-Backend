using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Auth;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.TwoFactor
{
    public class Validate2FALoginUseCase
    {
        private readonly ISaasUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private readonly ITwoFactorBackupCodeRepository _backupCodeRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILoginSessionRepository _loginSessionRepository;

        public Validate2FALoginUseCase(
            ISaasUserRepository userRepository,
            ITotpService totpService,
            ITwoFactorBackupCodeRepository backupCodeRepository,
            IJwtTokenService jwtTokenService,
            ILoginSessionRepository loginSessionRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _totpService = totpService ?? throw new ArgumentNullException(nameof(totpService));
            _backupCodeRepository = backupCodeRepository ?? throw new ArgumentNullException(nameof(backupCodeRepository));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _loginSessionRepository = loginSessionRepository ?? throw new ArgumentNullException(nameof(loginSessionRepository));
        }

        public async Task<Result<LoginResponse>> ExecuteAsync(Guid userId, string code, string deviceInfo, string ipAddress, CancellationToken ct = default)
        {
            // 1. Obtener usuario
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null)
                return Result<LoginResponse>.Fail("User not found");

            if (!user.TwoFactorEnabled)
                return Result<LoginResponse>.Fail("2FA is not enabled for this user");

            bool isValid = false;

            // 2. Intentar validar como TOTP first
            // Nota: Podríamos intentar detectar formato, pero TOTP es 6 dígitos numéricos.
            // Backup code es 8-10 chars.
            if (code.Length == 6 && int.TryParse(code, out _))
            {
                if (!string.IsNullOrWhiteSpace(user.TwoFactorSecret))
                {
                    isValid = _totpService.ValidateCode(user.TwoFactorSecret, code);
                }
            }
            // 3. Si no es TOTP válido, intentar como backup code
            if (!isValid)
            {
                // Limpiar guiones si el usuario los puso
                var cleanCode = code.Replace("-", "").ToUpperInvariant();
                // O mantener formato si almacenamos con guiones (nuestra impl guarda XXXX-XXXX)
                // Vamos a intentar validar tal cual recibimos o formateado
                
                // Nuestra impl de repo espera el código exacto almacenado.
                // Intentaremos validar el código tal cual (asumiendo formato correcto) o normalizando.
                // El repo ValidateAndUseBackupCodeAsync valida existencia y marca usado.
                isValid = await _backupCodeRepository.ValidateAndUseBackupCodeAsync(userId, "SaasUser", code.ToUpperInvariant(), ct);
            }

            if (!isValid)
            {
                return Result<LoginResponse>.Fail("Invalid 2FA code");
            }

            // 4. Generar tokens finales
            var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, "User");
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            var tokenHash = _jwtTokenService.ComputeTokenHash(refreshToken);

            // 5. Crear sesión
            var session = new LoginSession(
                user.Id,
                tokenHash,
                deviceInfo,
                ipAddress,
                DateTimeOffset.UtcNow.AddDays(7)
            );
            await _loginSessionRepository.AddAsync(session, ct);

            // 6. Actualizar login
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
    }
}
