using System;
using MeetLines.Domain.Enums;

namespace MeetLines.Domain.Entities
{
    public class SaasUser
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string? PasswordHash { get; private set; }
        public string? Phone { get; private set; }
        public string Timezone { get; private set; }
        public UserStatus Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        
        // ===== NUEVOS CAMPOS PARA AUTENTICACIÓN COMPLETA =====
        public bool IsEmailVerified { get; private set; }
        public AuthProvider AuthProvider { get; private set; }
        public string? ExternalProviderId { get; private set; }
        public string? ProfilePictureUrl { get; private set; }
        public DateTimeOffset? LastLoginAt { get; private set; }
        
        // Two-Factor Authentication
        public bool TwoFactorEnabled { get; private set; }
        public string? TwoFactorSecret { get; private set; }

        // Constructor para EF Core (Soluciona CS8618)
        // Usamos null! (null-forgiving operator) porque EF Core asignará los valores vía reflexión.
        private SaasUser() 
        {
            Name = null!;
            Email = null!;
            Timezone = null!;
        } 

        // Constructor privado principal para reutilizar lógica
        private SaasUser(
            Guid id, 
            string name, 
            string email, 
            string? passwordHash, 
            string timezone, 
            UserStatus status, 
            AuthProvider authProvider, 
            string? externalProviderId, 
            bool isEmailVerified,
            string? profilePictureUrl)
        {
            Id = id;
            Name = name;
            Email = email.ToLowerInvariant();
            PasswordHash = passwordHash;
            Timezone = timezone;
            Status = status;
            AuthProvider = authProvider;
            ExternalProviderId = externalProviderId;
            IsEmailVerified = isEmailVerified;
            ProfilePictureUrl = profilePictureUrl;
            
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // ===== FACTORY METHOD PARA REGISTRO LOCAL =====
        public static SaasUser CreateLocalUser(string name, string email, string passwordHash, string timezone = "UTC")
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("PasswordHash cannot be empty", nameof(passwordHash));

            // Llamamos al constructor privado en lugar de usar inicializador de objetos
            return new SaasUser(
                id: Guid.NewGuid(),
                name: name,
                email: email,
                passwordHash: passwordHash,
                timezone: timezone,
                status: UserStatus.Active,
                authProvider: AuthProvider.Local,
                externalProviderId: null,
                isEmailVerified: false,
                profilePictureUrl: null
            );
        }

        // ===== FACTORY METHOD PARA OAUTH (GOOGLE/FACEBOOK) =====
        public static SaasUser CreateOAuthUser(
            string name, 
            string email, 
            AuthProvider provider, 
            string externalProviderId,
            string? profilePictureUrl = null,
            string timezone = "UTC")
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(externalProviderId)) throw new ArgumentException("ExternalProviderId cannot be empty", nameof(externalProviderId));

            return new SaasUser(
                id: Guid.NewGuid(),
                name: name,
                email: email,
                passwordHash: null, // OAuth users no tienen password
                timezone: timezone,
                status: UserStatus.Active,
                authProvider: provider,
                externalProviderId: externalProviderId,
                isEmailVerified: true, // Asumimos verificado si viene de Google/FB
                profilePictureUrl: profilePictureUrl
            );
        }

        // ===== MÉTODOS DE COMPORTAMIENTO (DOMAIN LOGIC) =====
        public void UpdateProfile(string name, string? phone, string timezone)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            Name = name;
            Phone = phone;
            Timezone = timezone;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void ChangePassword(string newPasswordHash)
        {
             if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("PasswordHash cannot be empty", nameof(newPasswordHash));
             PasswordHash = newPasswordHash;
             UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void VerifyEmail()
        {
            IsEmailVerified = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateProfilePicture(string profilePictureUrl)
        {
            ProfilePictureUrl = profilePictureUrl;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void EnableTwoFactor(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("Two-factor secret cannot be empty", nameof(secret));
            
            TwoFactorSecret = secret;
            TwoFactorEnabled = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void DisableTwoFactor()
        {
            TwoFactorEnabled = false;
            TwoFactorSecret = null;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // Métodos de ayuda (Helpers)
        public bool CanLogin() => Status == UserStatus.Active;
        
        public bool RequiresPassword() => AuthProvider == AuthProvider.Local;
    }
}