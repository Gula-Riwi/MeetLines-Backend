using System;

namespace MeetLines.Application.Services.Interfaces
{
    public interface ITenantQueryFilter
    {
        Guid? GetCurrentTenantId();
        string? GetCurrentSubdomain();
        bool HasActiveTenant();
    }
}
