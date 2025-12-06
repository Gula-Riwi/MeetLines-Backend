using System;
using Microsoft.AspNetCore.Http;
using MeetLines.Application.Services.Interfaces;

namespace MeetLines.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string TenantIdKey = "TenantId";
        private const string SubdomainKey = "Subdomain";

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Guid? GetCurrentTenantId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            if (context.Items.TryGetValue(TenantIdKey, out var tenantIdObj) && tenantIdObj is Guid tenantId)
            {
                return tenantId;
            }

            return null;
        }

        public string? GetCurrentSubdomain()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            if (context.Items.TryGetValue(SubdomainKey, out var subdomainObj) && subdomainObj is string subdomain)
            {
                return subdomain;
            }

            return null;
        }

        public void SetTenant(Guid tenantId, string subdomain)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Items[TenantIdKey] = tenantId;
                context.Items[SubdomainKey] = subdomain;
            }
        }
    }
}
