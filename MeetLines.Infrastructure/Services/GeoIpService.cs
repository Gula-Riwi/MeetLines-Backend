using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MeetLines.Application.Services.Interfaces;

namespace MeetLines.Infrastructure.Services
{
    public class GeoIpOptions
    {
        public string DatabasePath { get; set; } = string.Empty;
        public TimeSpan CacheTtl { get; set; } = TimeSpan.FromHours(24);
    }

    public class GeoIpService : MeetLines.Application.Services.Interfaces.IGeoIpService, IDisposable
    {
        private readonly MaxMind.GeoIP2.DatabaseReader _reader;
        private readonly IMemoryCache _cache;
        private readonly GeoIpOptions _options;

        public GeoIpService(IOptions<GeoIpOptions> options, IMemoryCache cache)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            if (string.IsNullOrWhiteSpace(_options.DatabasePath) || !File.Exists(_options.DatabasePath))
            {
                throw new FileNotFoundException("GeoIP database not found. Configure GeoIp:DatabasePath in appsettings and provide the MaxMind DB file.");
            }

            _reader = new MaxMind.GeoIP2.DatabaseReader(_options.DatabasePath);
        }

        public Task<string?> GetTimezoneAsync(string ip, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(ip)) return Task.FromResult<string?>(null);

            if (_cache.TryGetValue(ip, out string? tz))
            {
                return Task.FromResult<string?>(tz);
            }

            try
            {
                var city = _reader.City(ip);
                tz = city?.Location?.TimeZone;
                if (!string.IsNullOrWhiteSpace(tz))
                {
                    _cache.Set(ip, tz, DateTimeOffset.UtcNow.Add(_options.CacheTtl));
                }

                return Task.FromResult<string?>(tz);
            }
            catch (Exception)
            {
                // No se pudo determinar timezone
                return Task.FromResult<string?>(null);
            }
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
