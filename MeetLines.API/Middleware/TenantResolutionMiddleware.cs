using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;
using MeetLines.Domain.ValueObjects;

namespace MeetLines.API.Middleware
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public TenantResolutionMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var host = context.Request.Host.Host;
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            
            // ====== RUTAS QUE NO REQUIEREN TENANT ======
            var publicRoutes = new[]
            {
                // ===== AUTH (Todas públicas EXCEPTO employee-login) =====
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/refresh-token",
                "/api/auth/logout",
                "/api/auth/verify-email",
                "/api/auth/forgot-password",
                "/api/auth/reset-password",
                "/api/auth/resend-verification-email",
                "/api/auth/oauth/discord",
                "/api/auth/oauth/facebook",
                "/api/auth/oauth-login",
                // "/api/auth/employee-login",  // ❌ REQUIERE TENANT (validar que el empleado pertenezca al proyecto)
                "/api/auth/create-transfer",
                "/api/auth/accept-transfer",
                
                // ===== PROFILE (Requiere auth pero NO tenant) =====
                "/api/profile",
                
                // ===== PROJECTS (Para crear proyecto, requiere auth pero NO tenant) =====
                "/api/projects",  // GET y POST
                "/api/projects/phone-number/",  // Para n8n obtener credenciales dinámicamente
                
                // ===== PROJECT CREDENTIALS (Para n8n obtener credenciales con API key) =====
                "/api/project-credentials/",  // GET credenciales por projectId + API key
                
                // ===== HEALTH =====
                "/health",
                "/api/health",
                
                // ===== WEBHOOKS =====
                "/webhook/whatsapp",
                "/webhook/",
                "/whatsapp", // n8n may call the top-level /whatsapp path (keep as public)
                
                // ===== WHATSAPP =====
                "/api/whatsapp/send-message",  // Recibe respuesta de n8n
                
                // ===== EMAIL TEMPLATES (Públicas) =====
                "/api/emailtemplates"
            };

            // Si la ruta está en la lista de excepciones, NO aplicar tenant resolution
            if (publicRoutes.Any(route => path.StartsWith(route, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Special-case: allow paths like "/{tenantId}/whatsapp" where n8n may prefix a tenant id
            // e.g. GET https://services.meet-lines.com/1ede30b1-366e-4c61-b648-b68cf3930d40/whatsapp
            var segments = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2 && segments[1].Equals("whatsapp", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Lee del appsettings.json que obtiene valores del .env
            var baseDomain = _configuration["Multitenancy:BaseDomain"] ?? "meet-lines.com";

            // Si el host no termina con el dominio base, o es igual al dominio base, no hay subdominio
            if (!host.EndsWith(baseDomain, StringComparison.OrdinalIgnoreCase) || 
                host.Equals(baseDomain, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Extraer subdominio
            // host: subdomain.meet-lines.com
            // base: meet-lines.com
            // length diff: subdomain. (includes dot)
            var subdomainPart = host.Substring(0, host.Length - baseDomain.Length - 1);
            
            // Validar si es un subdominio reservado o inválido
            if (!SubdomainValidator.IsValid(subdomainPart, out _))
            {
                // Si es inválido, simplemente continuamos sin tenant (o podríamos retornar 404)
                await _next(context);
                return;
            }

            // Resolver tenant desde base de datos
            // Usamos IServiceProvider para crear un scope porque el repositorio es Scoped
            var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
            var projectRepository = context.RequestServices.GetRequiredService<IProjectRepository>();

            var project = await projectRepository.GetBySubdomainAsync(subdomainPart);

            if (project != null && project.Status == "active")
            {
                tenantService.SetTenant(project.Id, project.Subdomain);
            }
            else
            {
                // Si el subdominio existe pero no encontramos proyecto, retornamos 404 Not Found
                // Opcionalmente podríamos redirigir al home
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Tenant not found");
                return;
            }

            await _next(context);
        }
    }
}
