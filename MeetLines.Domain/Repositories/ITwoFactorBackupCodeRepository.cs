using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface ITwoFactorBackupCodeRepository
    {
        /// <summary>
        /// Genera y guarda códigos de respaldo para un usuario
        /// </summary>
        Task<List<TwoFactorBackupCode>> GenerateBackupCodesAsync(
            Guid userId, 
            string userType, 
            int count = 10, 
            CancellationToken ct = default);

        /// <summary>
        /// Valida y marca como usado un código de respaldo
        /// </summary>
        Task<bool> ValidateAndUseBackupCodeAsync(
            Guid userId, 
            string userType, 
            string code, 
            CancellationToken ct = default);

        /// <summary>
        /// Obtiene todos los códigos de respaldo de un usuario (usados y no usados)
        /// </summary>
        Task<List<TwoFactorBackupCode>> GetByUserAsync(
            Guid userId, 
            string userType, 
            CancellationToken ct = default);

        /// <summary>
        /// Elimina todos los códigos de respaldo de un usuario
        /// </summary>
        Task DeleteAllByUserAsync(
            Guid userId, 
            string userType, 
            CancellationToken ct = default);

        /// <summary>
        /// Cuenta cuántos códigos no usados tiene un usuario
        /// </summary>
        Task<int> CountUnusedCodesAsync(
            Guid userId, 
            string userType, 
            CancellationToken ct = default);
    }
}
