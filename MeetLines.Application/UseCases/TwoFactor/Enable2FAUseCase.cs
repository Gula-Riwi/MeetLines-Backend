using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.TwoFactor;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.TwoFactor
{
    public class Enable2FAUseCase
    {
        private readonly ISaasUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private readonly ITwoFactorBackupCodeRepository _backupCodeRepository;

        public Enable2FAUseCase(
            ISaasUserRepository userRepository,
            ITotpService totpService,
            ITwoFactorBackupCodeRepository backupCodeRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _totpService = totpService ?? throw new ArgumentNullException(nameof(totpService));
            _backupCodeRepository = backupCodeRepository ?? throw new ArgumentNullException(nameof(backupCodeRepository));
        }

        public async Task<Enable2FAResponse> ExecuteAsync(Guid userId, CancellationToken ct = default)
        {
            // 1. Obtener usuario
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null)
                throw new ArgumentException("User not found");

            // 2. Verificar que no tenga 2FA ya habilitado
            if (user.TwoFactorEnabled)
                throw new InvalidOperationException("2FA is already enabled for this user");

            // 3. Generar secreto TOTP
            var secret = _totpService.GenerateSecret();

            // 4. Generar URI para QR code
            var qrCodeUri = _totpService.GenerateQrCodeUri(user.Email, secret);

            // 5. Generar códigos de respaldo
            var backupCodesEntities = await _backupCodeRepository.GenerateBackupCodesAsync(
                userId, 
                "SaasUser", 
                10, 
                ct);

            var backupCodes = backupCodesEntities.ConvertAll(bc => bc.Code);

            // 6. Habilitar 2FA en el usuario (guardar el secreto)
            user.EnableTwoFactor(secret);
            await _userRepository.UpdateAsync(user, ct);

            // 7. Retornar respuesta
            return new Enable2FAResponse
            {
                Secret = secret,
                QrCodeUri = qrCodeUri,
                BackupCodes = backupCodes
            };
        }
    }
}
