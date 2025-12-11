using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    /// <summary>
    /// Repositorio para gestionar proyectos/empresas del usuario
    /// </summary>
    public interface IProjectRepository
    {
        /// <summary>
        /// Obtiene un proyecto por ID
        /// </summary>
        Task<Project?> GetAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Obtiene todos los proyectos de un usuario
        /// </summary>
        Task<IEnumerable<Project>> GetByUserAsync(Guid userId, CancellationToken ct = default);

        /// <summary>
        /// Obtiene todos los proyectos activos del sistema (Público)
        /// </summary>
        Task<IEnumerable<Project>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Obtiene la cantidad de proyectos activos de un usuario
        /// </summary>
        Task<int> GetActiveCountByUserAsync(Guid userId, CancellationToken ct = default);

        /// <summary>
        /// Crea un nuevo proyecto
        /// </summary>
        Task AddAsync(Project project, CancellationToken ct = default);

        /// <summary>
        /// Actualiza un proyecto existente
        /// </summary>
        Task UpdateAsync(Project project, CancellationToken ct = default);

        /// <summary>
        /// Elimina un proyecto (cambiar estado)
        /// </summary>
        Task DeleteAsync(Guid projectId, CancellationToken ct = default);

        /// <summary>
        /// Verifica si un usuario tiene permiso sobre un proyecto
        /// </summary>
        Task<bool> IsUserProjectOwnerAsync(Guid userId, Guid projectId, CancellationToken ct = default);

        /// <summary>
        /// Obtiene un proyecto por su subdominio
        /// </summary>
        Task<Project?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default);

        /// <summary>
        /// Obtiene un proyecto por su whatsapp phone_number_id
        /// </summary>
        Task<Project?> GetByWhatsappPhoneNumberIdAsync(string phoneNumberId, CancellationToken ct = default);

        /// <summary>
        /// Obtiene un proyecto por su whatsapp verify token
        /// </summary>
        Task<Project?> GetByWhatsappVerifyTokenAsync(string verifyToken, CancellationToken ct = default);

        /// <summary>
        /// Verifica si existe un subdominio (para validación de unicidad)
        /// </summary>
        Task<bool> ExistsSubdomainAsync(string subdomain, CancellationToken ct = default);
    }
}
