using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.TwoFactor;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.TwoFactor
{
    public class Disable2FAUseCase
    {
        private readonly ISaasUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private readonly ITwoFactorBackupCodeRepository _backupCodeRepository;

        public Disable2FAUseCase(
            ISaasUserRepository userRepository,
            ITotpService totpService,
            ITwoFactorBackupCodeRepository backupCodeRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _totpService = totpService ?? throw new ArgumentNullException(nameof(totpService));
            _backupCodeRepository = backupCodeRepository ?? throw new ArgumentNullException(nameof(backupCodeRepository));
        }

        public async Task<TwoFactorResponse> ExecuteAsync(Guid userId, string code, CancellationToken ct = default)
        {
            // 1. Obtener usuario
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null)
                throw new ArgumentException("User not found");

            // 2. Verificar que tenga 2FA habilitado
            if (!user.TwoFactorEnabled)
                throw new InvalidOperationException("2FA is not enabled for this user");

            // 3. Validar el código TOTP
            if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
                throw new InvalidOperationException("2FA secret is missing");

            var isValid = _totpService.ValidateCode(user.TwoFactorSecret, code);
            if (!isValid)
            {
                return new TwoFactorResponse
                {
                    Success = false,
                    Message = "Invalid 2FA code"
                };
            }

            // 4. Deshabilitar 2FA
            user.DisableTwoFactor();
            await _userRepository.UpdateAsync(user, ct);

            // 5. Eliminar códigos de respaldo
            await _backupCodeRepository.DeleteAllByUserAsync(userId, "SaasUser", ct);

            return new TwoFactorResponse
            {
                Success = true,
                Message = "2FA has been disabled successfully"
            };
        }
    }
}
