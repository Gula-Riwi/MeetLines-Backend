using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Projects;

namespace MeetLines.Application.UseCases.Projects.Interfaces
{
    /// <summary>
    /// Use case para configurar la integración de Telegram en un proyecto
    /// Sigue el mismo patrón que IConfigureWhatsappUseCase
    /// </summary>
    public interface IConfigureTelegramUseCase
    {
        /// <summary>
        /// Configura la integración de Telegram para un proyecto existente
        /// </summary>
        /// <param name="userId">ID del usuario propietario del proyecto</param>
        /// <param name="projectId">ID del proyecto a configurar</param>
        /// <param name="request">Datos de configuración de Telegram</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Resultado con los datos actualizados del proyecto</returns>
        Task<Result<ProjectResponse>> ExecuteAsync(
            Guid userId,
            Guid projectId,
            ConfigureTelegramRequest request,
            CancellationToken ct = default);
    }
}
