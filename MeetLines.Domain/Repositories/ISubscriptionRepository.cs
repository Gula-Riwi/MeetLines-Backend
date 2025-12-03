using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    /// <summary>
    /// Repositorio para gestionar suscripciones
    /// </summary>
    public interface ISubscriptionRepository
    {
        /// <summary>
        /// Obtiene la suscripción activa de un usuario
        /// </summary>
        Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

        /// <summary>
        /// Obtiene una suscripción por ID
        /// </summary>
        Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken ct = default);

        /// <summary>
        /// Crea una nueva suscripción
        /// </summary>
        Task AddAsync(Subscription subscription, CancellationToken ct = default);

        /// <summary>
        /// Actualiza una suscripción existente
        /// </summary>
        Task UpdateAsync(Subscription subscription, CancellationToken ct = default);
    }
}
