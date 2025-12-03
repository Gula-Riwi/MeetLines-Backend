using System;
using MeetLines.Application.Services;

namespace MeetLines.Infrastructure.Services
{
    /// <summary>
    /// Servicio para construir filtros de queries con información del tenant actual
    /// Usado por los repositorios para asegurar que solo acceden a datos del tenant activo
    /// </summary>
    public class TenantQueryFilter : ITenantQueryFilter
    {
        private readonly ITenantService _tenantService;

        public TenantQueryFilter(ITenantService tenantService)
        {
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        }

        /// <summary>
        /// Obtiene el ID del tenant actual
        /// Retorna null si no hay tenant resuelto (solicitud a dominio base)
        /// </summary>
        public Guid? GetCurrentTenantId()
        {
            return _tenantService.GetCurrentTenantId();
        }

        /// <summary>
        /// Obtiene el subdominio del tenant actual
        /// </summary>
        public string? GetCurrentSubdomain()
        {
            return _tenantService.GetCurrentSubdomain();
        }

        /// <summary>
        /// Verifica si hay un tenant activo
        /// </summary>
        public bool HasActiveTenant()
        {
            return _tenantService.GetCurrentTenantId().HasValue;
        }
    }
}
