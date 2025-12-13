using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Códigos de respaldo para autenticación de dos factores
    /// Permite a los usuarios acceder si pierden acceso a su app de autenticación
    /// </summary>
    public class TwoFactorBackupCode
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        
        /// <summary>
        /// Tipo de usuario: "AppUser" o "SaasUser"
        /// </summary>
        public string UserType { get; private set; }
        
        /// <summary>
        /// Código de respaldo (8-10 caracteres alfanuméricos)
        /// </summary>
        public string Code { get; private set; }
        
        /// <summary>
        /// Indica si el código ya fue utilizado
        /// </summary>
        public bool IsUsed { get; private set; }
        
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UsedAt { get; private set; }

        // Constructor para EF Core
        private TwoFactorBackupCode()
        {
            UserType = null!;
            Code = null!;
        }

        public TwoFactorBackupCode(Guid userId, string userType, string code)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty", nameof(userId));
            
            if (string.IsNullOrWhiteSpace(userType))
                throw new ArgumentException("UserType cannot be empty", nameof(userType));
            
            if (userType != "AppUser" && userType != "SaasUser")
                throw new ArgumentException("UserType must be 'AppUser' or 'SaasUser'", nameof(userType));
            
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Code cannot be empty", nameof(code));

            Id = Guid.NewGuid();
            UserId = userId;
            UserType = userType;
            Code = code;
            IsUsed = false;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Marca el código como utilizado
        /// </summary>
        public void MarkAsUsed()
        {
            if (IsUsed)
                throw new InvalidOperationException("Backup code has already been used");
            
            IsUsed = true;
            UsedAt = DateTimeOffset.UtcNow;
        }
    }
}
