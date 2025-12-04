using MeetLines.Application.DTOs.Common;

namespace MeetLines.Application.Services.Interfaces
{
    /// <summary>
    /// Servicio para capturar información del contexto HTTP automáticamente
    /// (IP Address, Device Info, Timezone, etc.)
    /// </summary>
    public interface IHttpContextInfoService
    {
        /// <summary>
        /// Obtiene toda la información disponible del contexto HTTP actual
        /// </summary>
        HttpContextInfo GetContextInfo();

        /// <summary>
        /// Obtiene la IP del cliente desde el contexto HTTP
        /// Respeta headers de proxy como X-Forwarded-For
        /// </summary>
        string? GetClientIpAddress();

        /// <summary>
        /// Obtiene el User-Agent (Device Info) del cliente
        /// </summary>
        string? GetDeviceInfo();

        /// <summary>
        /// Obtiene el timezone desde los headers HTTP
        /// Intenta varios headers comunes: Timezone, Time-Zone, X-Timezone, X-Time-Zone
        /// </summary>
        string? GetTimezone();
    }
}
