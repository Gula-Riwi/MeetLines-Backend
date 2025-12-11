using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OAuth;
using MeetLines.Infrastructure.Data;
using MeetLines.Infrastructure.Repositories;
using MeetLines.Infrastructure.Services;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;
using MeetLines.Application.Services;


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
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IChannelRepository, ChannelRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IAppUserRepository, AppUserRepository>();

            // WhatsApp Bot System Repositories
            services.AddScoped<IProjectBotConfigRepository, ProjectBotConfigRepository>();
            services.AddScoped<IKnowledgeBaseRepository, KnowledgeBaseRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<ICustomerFeedbackRepository, CustomerFeedbackRepository>();
            services.AddScoped<ICustomerReactivationRepository, CustomerReactivationRepository>();
            services.AddScoped<IBotMetricsRepository, BotMetricsRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();

            // Servicios Multitenancy
            services.AddHttpContextAccessor();
            services.AddScoped<MeetLines.Application.Services.Interfaces.ITenantService, TenantService>();
            services.AddScoped<MeetLines.Application.Services.Interfaces.ITenantQueryFilter, TenantQueryFilter>();

            // Memory cache (used by GeoIP service)
            services.AddMemoryCache();

            // GeoIP service (MaxMind DB) - implementation bound to Application interface
            services.AddSingleton<MeetLines.Application.Services.Interfaces.IGeoIpService, MeetLines.Infrastructure.Services.GeoIpService>();
            
            // Servicio de Discord Webhooks con HttpClient
            services.AddHttpClient<IDiscordWebhookService, DiscordWebhookService>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Servicios de autenticación
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton<IEmailTemplateBuilder, EmailTemplateBuilder>();
            services.AddScoped<MeetLines.Domain.Repositories.ITransferTokenRepository, MeetLines.Infrastructure.Repositories.TransferTokenRepository>();

            // Business Services
            services.AddScoped<IAppointmentAssignmentService, MeetLines.Application.Services.AppointmentAssignmentService>();
            
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
            })

            // Discord OAuth using generic OAuth handler
            // Discord endpoints: https://discord.com/api/oauth2/
            // Scopes: identify + email (email returned only if scope 'email' is granted)
            .AddOAuth("Discord", options =>
            {
                options.ClientId = configuration["Authentication:Discord:ClientId"] ?? Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID") ?? "";
                options.ClientSecret = configuration["Authentication:Discord:ClientSecret"] ?? Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET") ?? "";
                options.CallbackPath = "/signin-discord";

                options.AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
                options.TokenEndpoint = "https://discord.com/api/oauth2/token";
                options.UserInformationEndpoint = "https://discord.com/api/users/@me";

                options.Scope.Clear();
                options.Scope.Add("identify");
                options.Scope.Add("email");

                options.SaveTokens = true;

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
                        var root = payload.RootElement;

                        var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;

                        if (root.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                        {
                            var idVal = idProp.GetString();
                            if (!string.IsNullOrEmpty(idVal) && identity != null)
                                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, idVal));
                        }

                        // Discord username is separate from discriminator. We'll add the plain username.
                        if (root.TryGetProperty("username", out var usernameProp) && usernameProp.ValueKind == JsonValueKind.String)
                        {
                            var uname = usernameProp.GetString();
                            if (!string.IsNullOrEmpty(uname) && identity != null)
                                identity.AddClaim(new Claim(ClaimTypes.Name, uname));
                        }

                        if (root.TryGetProperty("email", out var emailProp) && emailProp.ValueKind == JsonValueKind.String)
                        {
                            var emailVal = emailProp.GetString();
                            if (!string.IsNullOrEmpty(emailVal) && identity != null)
                                identity.AddClaim(new Claim(ClaimTypes.Email, emailVal));
                        }
                    }
                };
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