using System.Collections.Generic;

namespace MeetLines.Application.Services.Interfaces
{
    /// <summary>
    /// Servicio para generar y validar códigos TOTP (Time-based One-Time Password)
    /// Compatible con Google Authenticator, Microsoft Authenticator, Authy, etc.
    /// </summary>
    public interface ITotpService
    {
        /// <summary>
        /// Genera un secreto aleatorio para TOTP (Base32)
        /// </summary>
        string GenerateSecret();

        /// <summary>
        /// Genera la URI para el código QR que el usuario escanea con su app
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="secret">Secreto TOTP generado</param>
        /// <param name="issuer">Nombre de la aplicación (ej: "MeetLines")</param>
        string GenerateQrCodeUri(string email, string secret, string issuer = "MeetLines");

        /// <summary>
        /// Valida un código TOTP ingresado por el usuario
        /// </summary>
        /// <param name="secret">Secreto TOTP del usuario</param>
        /// <param name="code">Código de 6 dígitos ingresado</param>
        /// <param name="timeToleranceSeconds">Tolerancia de tiempo en segundos (default: 30s)</param>
        bool ValidateCode(string secret, string code, int timeToleranceSeconds = 30);

        /// <summary>
        /// Genera códigos de respaldo aleatorios
        /// </summary>
        /// <param name="count">Cantidad de códigos a generar</param>
        List<string> GenerateBackupCodes(int count = 10);
    }
}
