using MeetLines.Application.IoC;
using MeetLines.Infrastructure.IoC;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using MeetLines.API.Middleware;
using MeetLines.API.Middlewares;

// Determine environment
var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var envFileName = environmentName == "Development" ? ".env.development" : ".env";



// Helper searching upwards
string? FindFileUpwards(string fileName)
{
    var dir = Directory.GetCurrentDirectory();
    while (!string.IsNullOrEmpty(dir))
    {
        var path = Path.Combine(dir, fileName);
        if (File.Exists(path)) return path;
        var parent = Directory.GetParent(dir);
        if (parent == null) break;
        dir = parent.FullName;
    }
    return null;
}

var envPathToLoad = FindFileUpwards(envFileName);

if (envPathToLoad != null)
{
    Console.WriteLine($"🌎 Environment: {environmentName}");
    Console.WriteLine($"🌎 Cargando {envFileName} desde: {envPathToLoad}");
    DotNetEnv.Env.Load(envPathToLoad);
    // Verificar carga
    Console.WriteLine($"🌎 DB_HOST cargado: {Environment.GetEnvironmentVariable("DB_HOST")}");
}
else
{
    Console.WriteLine($"⚠️ Environment: {environmentName}");
    Console.WriteLine($"⚠️ No se encontró archivo {envFileName} buscando hacia arriba desde {Directory.GetCurrentDirectory()}");
    
    // Fallback: Try loading .env if .env.development was missing in Dev mode
    if (environmentName == "Development")
    {
         Console.WriteLine("⚠️ Intentando fallback a .env normal...");
         var fallbackPath = FindFileUpwards(".env");
         if (fallbackPath != null)
         {
             DotNetEnv.Env.Load(fallbackPath);
             Console.WriteLine($"🌎 Fallback: Cargado .env desde {fallbackPath}");
         }
    }
}

var builder = WebApplication.CreateBuilder(args);

// Expande variables de entorno en la cadena de conexión
// Expande variables de entorno en la configuración (Manual para soporte ${VAR})
// Esto es necesario porque Environment.ExpandEnvironmentVariables no soporta ${VAR} en Windows por defecto
// y appsettings.json usa esa sintaxis.

string ExpandVariables(string value)
{
    if (string.IsNullOrEmpty(value)) return value;
    return System.Text.RegularExpressions.Regex.Replace(value, @"\$\{([a-zA-Z_][a-zA-Z0-9_]*)\}", match =>
    {
        var envVar = match.Groups[1].Value;
        return Environment.GetEnvironmentVariable(envVar) ?? match.Value;
    });
}

// 1. Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString != null)
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] = ExpandVariables(connectionString);
    Console.WriteLine("📊 ConnectionString expandido");
}

// 2. JWT
var jwtSecret = builder.Configuration["Jwt:SecretKey"];
if (jwtSecret != null) builder.Configuration["Jwt:SecretKey"] = ExpandVariables(jwtSecret);

// 3. Email
builder.Configuration["Email:SmtpUser"] = ExpandVariables(builder.Configuration["Email:SmtpUser"] ?? "");
builder.Configuration["Email:SmtpPassword"] = ExpandVariables(builder.Configuration["Email:SmtpPassword"] ?? "");
builder.Configuration["Email:FromEmail"] = ExpandVariables(builder.Configuration["Email:FromEmail"] ?? "");

// 4. Auth (Social)
builder.Configuration["Authentication:Google:ClientId"] = ExpandVariables(builder.Configuration["Authentication:Google:ClientId"] ?? "");
builder.Configuration["Authentication:Google:ClientSecret"] = ExpandVariables(builder.Configuration["Authentication:Google:ClientSecret"] ?? "");
builder.Configuration["Authentication:Facebook:AppId"] = ExpandVariables(builder.Configuration["Authentication:Facebook:AppId"] ?? "");
builder.Configuration["Authentication:Facebook:AppSecret"] = ExpandVariables(builder.Configuration["Authentication:Facebook:AppSecret"] ?? "");

// 5. Multitenancy
builder.Configuration["Multitenancy:BaseDomain"] = ExpandVariables(builder.Configuration["Multitenancy:BaseDomain"] ?? "");
builder.Configuration["Multitenancy:Protocol"] = ExpandVariables(builder.Configuration["Multitenancy:Protocol"] ?? "");

// Register application services
builder.Services.AddApplicationServices();

// Register infrastructure (DB, repositories, services, JWT auth)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Controllers
builder.Services.AddControllers();

// ===== CONFIGURAR FLUENTVALIDATION =====
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<MeetLines.Application.Validators.RegisterRequestValidator>();

// Configure API Behavior for validation errors
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new
        {
            success = false,
            message = "Errores de validación",
            errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "MeetLines API", 
        Version = "v1",
        Description = "API de autenticación con Clean Architecture"
    });
    
    // ApiKey para permitir control total sobre el header "Authorization"
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticación JWT usando el esquema Bearer.\r\n\r\nIngrese la palabra 'Bearer' seguida de un espacio y su token.\r\n\r\nEjemplo: \"Bearer eyJhbGciOi...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

app.UseCors(builder => builder
    .SetIsOriginAllowed(origin =>
    {
        if (string.IsNullOrEmpty(origin)) return false;
        // Parse origin safely
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;

        var host = uri.Host ?? string.Empty;

        // Allow localhost and loopback (any port)
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) return true;
        if (host.Equals("127.0.0.1")) return true;

        // Allow subdomains of meet-lines.local and production domain (any port)
        if (host.EndsWith(".meet-lines.local", StringComparison.OrdinalIgnoreCase)) return true;
        if (host.Equals("meet-lines.local", StringComparison.OrdinalIgnoreCase)) return true;
        if (host.EndsWith(".meet-lines.com", StringComparison.OrdinalIgnoreCase)) return true;

        return false;
    })
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MeetLines API v1");
    });
}

// === [NUEVO] MIDDLEWARE DE RESOLUCIÓN DE TENANT ===
// Se coloca antes de autenticación para resolver el tenant
app.UseMiddleware<TenantResolutionMiddleware>();
// ==================================================

// === [NUEVO] MIDDLEWARE DE ERRORES (DISCORD) ===
// Se coloca antes de HTTPS, Auth y Controllers para atrapar cualquier excepción
app.UseMiddleware<DiscordGlobalExceptionMiddleware>();
// ===============================================

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();