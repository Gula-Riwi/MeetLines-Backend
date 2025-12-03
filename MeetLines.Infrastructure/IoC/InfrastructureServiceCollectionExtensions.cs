using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MeetLines.Infrastructure.Data;
using MeetLines.Infrastructure.Repositories;
using MeetLines.Infrastructure.Services;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;

namespace MeetLines.Infrastructure.IoC
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Primero intentar obtener la conexión de variables de entorno
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
            var dbUsername = Environment.GetEnvironmentVariable("DB_USERNAME");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var dbDatabase = Environment.GetEnvironmentVariable("DB_DATABASE");

            string conn;
            
            if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbPort) && 
                !string.IsNullOrEmpty(dbUsername) && !string.IsNullOrEmpty(dbPassword) && 
                !string.IsNullOrEmpty(dbDatabase))
            {
                // Usar conexión desde variables de entorno
                conn = $"Host={dbHost};Port={dbPort};Database={dbDatabase};Username={dbUsername};Password={dbPassword}";
                Console.WriteLine($"📊 Conectando a BD remota: {dbHost}:{dbPort}/{dbDatabase}");
            }
            else
            {
                // Obtener de appsettings y reemplazar variables
                conn = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentException("DefaultConnection missing");
                
                // Reemplazar variables de template si existen
                conn = conn.Replace("${DB_HOST}", Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost");
                conn = conn.Replace("${DB_PORT}", Environment.GetEnvironmentVariable("DB_PORT") ?? "5432");
                conn = conn.Replace("${DB_DATABASE}", Environment.GetEnvironmentVariable("DB_DATABASE") ?? "meetline");
                conn = conn.Replace("${DB_USERNAME}", Environment.GetEnvironmentVariable("DB_USERNAME") ?? "postgres");
                conn = conn.Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres");
                
                Console.WriteLine($"📊 Conectando a BD local/configurable");
            }

            services.AddDbContext<MeetLinesPgDbContext>(o => o.UseNpgsql(conn));
            return services;
        }

        public static IServiceCollection AddInfrastructureRepositories(this IServiceCollection services)
        {
            // Registrar todos los repositorios
            services.AddScoped<ISaasUserRepository, SaasUserRepository>();
            services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            services.AddScoped<ILoginSessionRepository, LoginSessionRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Servicios de autenticación
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IEmailService, EmailService>();

            // Memory cache (used by GeoIP service)
            services.AddMemoryCache();

            // GeoIP service (MaxMind DB) - implementation bound to Application interface
            services.AddSingleton<MeetLines.Application.Services.Interfaces.IGeoIpService, MeetLines.Infrastructure.Services.GeoIpService>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var secretKey = configuration["Jwt:SecretKey"] ?? throw new ArgumentException("Jwt:SecretKey is missing");
            var issuer = configuration["Jwt:Issuer"] ?? "MeetLines";
            var audience = configuration["Jwt:Audience"] ?? "MeetLines";

            var key = Encoding.UTF8.GetBytes(secretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Cambiar a true en producción
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"] ?? "";
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "";
            })
            .AddFacebook(options =>
            {
                options.AppId = configuration["Authentication:Facebook:AppId"] ?? "";
                options.AppSecret = configuration["Authentication:Facebook:AppSecret"] ?? "";
            });

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind GeoIp options from configuration
            services.Configure<MeetLines.Infrastructure.Services.GeoIpOptions>(configuration.GetSection("GeoIp"));

            services.AddInfrastructureDatabase(configuration);
            services.AddInfrastructureRepositories();
            services.AddInfrastructureServices();
            services.AddJwtAuthentication(configuration);

            return services;
        }
    }
}