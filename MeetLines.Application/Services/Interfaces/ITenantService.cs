using System;

namespace MeetLines.Application.Services.Interfaces
{
    public interface ITenantService
    {
        Guid? GetCurrentTenantId();
        string? GetCurrentSubdomain();
        void SetTenant(Guid tenantId, string subdomain);
    }
}
