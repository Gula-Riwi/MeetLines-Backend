using Microsoft.AspNetCore.Http;
using MeetLines.Application.DTOs.Common;
using MeetLines.Application.Services.Interfaces;

namespace MeetLines.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio para capturar información del contexto HTTP
    /// </summary>
    public class HttpContextInfoService : IHttpContextInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextInfoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public HttpContextInfo GetContextInfo()
        {
            return new HttpContextInfo
            {
                IpAddress = GetClientIpAddress(),
                DeviceInfo = GetDeviceInfo(),
                Timezone = GetTimezone(),
                RawUserAgent = GetRawUserAgent()
            };
        }

        public string? GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Intentar obtener desde X-Forwarded-For (proxy/load balancer)
            var headers = context.Request.Headers;
            if (headers.ContainsKey("X-Forwarded-For"))
            {
                var xff = headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrWhiteSpace(xff))
                {
                    // El primer IP en la cadena es el cliente original
                    var firstIp = xff.Split(',')[0].Trim();
                    if (!string.IsNullOrEmpty(firstIp))
                        return firstIp;
                }
            }

            // Fallback a la conexión directa
            var remoteIp = context.Connection.RemoteIpAddress;
            return remoteIp?.ToString();
        }

        public string? GetDeviceInfo()
        {
            return GetRawUserAgent();
        }

        public string? GetTimezone()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var headers = context.Request.Headers;
            
            // Intentar varios headers comunes para timezone
            string? tz = null;
            
            if (headers.ContainsKey("Timezone"))
                tz = headers["Timezone"].ToString();
            
            if (string.IsNullOrWhiteSpace(tz) && headers.ContainsKey("Time-Zone"))
                tz = headers["Time-Zone"].ToString();
            
            if (string.IsNullOrWhiteSpace(tz) && headers.ContainsKey("X-Timezone"))
                tz = headers["X-Timezone"].ToString();
            
            if (string.IsNullOrWhiteSpace(tz) && headers.ContainsKey("X-Time-Zone"))
                tz = headers["X-Time-Zone"].ToString();

            return string.IsNullOrWhiteSpace(tz) ? null : tz;
        }

        private string? GetRawUserAgent()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            if (context.Request.Headers.TryGetValue("User-Agent", out var ua))
                return ua.ToString();
            
            return null;
        }
    }
}
