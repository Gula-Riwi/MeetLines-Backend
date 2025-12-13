using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using MeetLines.Application.Services.Interfaces;
using OtpNet;

namespace MeetLines.Infrastructure.Services
{
    public class TotpService : ITotpService
    {
        private const int SecretLength = 20; // 160 bits
        private const int CodeLength = 6;
        private const int BackupCodeLength = 8;

        public string GenerateSecret()
        {
            // Genera un secreto aleatorio de 160 bits (20 bytes)
            var key = KeyGeneration.GenerateRandomKey(SecretLength);
            
            // Convierte a Base32 (formato estándar para TOTP)
            return Base32Encoding.ToString(key);
        }

        public string GenerateQrCodeUri(string email, string secret, string issuer = "MeetLines")
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
            
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("Secret cannot be empty", nameof(secret));

            // Formato: otpauth://totp/Issuer:email?secret=SECRET&issuer=Issuer
            var label = $"{issuer}:{email}";
            return $"otpauth://totp/{Uri.EscapeDataString(label)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&digits={CodeLength}&period=30";
        }

        public bool ValidateCode(string secret, string code, int timeToleranceSeconds = 30)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("Secret cannot be empty", nameof(secret));
            
            if (string.IsNullOrWhiteSpace(code))
                return false;

            try
            {
                // Decodifica el secreto de Base32
                var secretBytes = Base32Encoding.ToBytes(secret);
                
                // Crea el generador TOTP (Otp.NET 1.4.1)
                var totp = new Totp(secretBytes);
                
                // Calcula la ventana de tiempo (cada paso es 30 segundos)
                var windowSteps = timeToleranceSeconds / 30;
                
                // Verifica el código actual y los pasos anteriores/futuros
                for (int i = -windowSteps; i <= windowSteps; i++)
                {
                    var expectedCode = totp.ComputeTotp(DateTime.UtcNow.AddSeconds(i * 30));
                    if (expectedCode == code)
                        return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GenerateBackupCodes(int count = 10)
        {
            if (count <= 0 || count > 50)
                throw new ArgumentException("Count must be between 1 and 50", nameof(count));

            var codes = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                codes.Add(GenerateBackupCode());
            }
            
            return codes;
        }

        private string GenerateBackupCode()
        {
            // Genera un código de 8 caracteres alfanuméricos
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var code = new char[BackupCodeLength];
            
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[BackupCodeLength];
                rng.GetBytes(bytes);
                
                for (int i = 0; i < BackupCodeLength; i++)
                {
                    code[i] = chars[bytes[i] % chars.Length];
                }
            }
            
            // Formato: XXXX-XXXX para mejor legibilidad
            return $"{new string(code, 0, 4)}-{new string(code, 4, 4)}";
        }
    }
}
