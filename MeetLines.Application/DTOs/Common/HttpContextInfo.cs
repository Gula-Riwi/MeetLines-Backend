namespace MeetLines.Application.DTOs.Common
{
    /// <summary>
    /// DTO que contiene información extraída del contexto HTTP
    /// </summary>
    public class HttpContextInfo
    {
        /// <summary>
        /// Dirección IP del cliente (respeta X-Forwarded-For)
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Información del dispositivo/navegador (User-Agent)
        /// </summary>
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// Timezone del cliente (desde headers HTTP)
        /// </summary>
        public string? Timezone { get; set; }

        /// <summary>
        /// User-Agent completo sin procesar
        /// </summary>
        public string? RawUserAgent { get; set; }
    }
}
