using System;

namespace MeetLines.Application.Services
{
    public interface ITenantQueryFilter
    {
        Guid? GetCurrentTenantId();
        string? GetCurrentSubdomain();
        bool HasActiveTenant();
    }
}
