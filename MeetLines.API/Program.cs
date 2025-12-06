using MeetLines.Application.IoC;
using MeetLines.Infrastructure.IoC;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
// Asegúrate de que el namespace coincida con donde creaste el archivo del Middleware
using MeetLines.API.Middlewares; 

// Determine environment
var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var envFileName = environmentName == "Development" ? ".env.development" : ".env";

// Load .env file from workspace root FIRST before anything else
var currentDir = Directory.GetCurrentDirectory();
var parentDir = Directory.GetParent(currentDir);
var workspaceRoot = parentDir?.FullName ?? currentDir; // Fallback to current if no parent (e.g. root drive)

var envPathRoot = Path.Combine(workspaceRoot, envFileName);
var envPathLocal = Path.Combine(currentDir, envFileName);

string? envPathToLoad = null;

// Logic: Check Parent first (Project Root when running from API folder), then Current (if running from Root)
if (File.Exists(envPathRoot))
{
    envPathToLoad = envPathRoot;
    Console.WriteLine($"🌎 Environment: {environmentName}");
    Console.WriteLine($"🌎 Cargando {envFileName} desde raíz del proyecto: {envPathRoot}");
}
else if (File.Exists(envPathLocal) && envPathLocal != envPathRoot)
{
    // Only check local if it's different (i.e., we are not already at root)
    envPathToLoad = envPathLocal;
    Console.WriteLine($"🌎 Environment: {environmentName}");
    Console.WriteLine($"🌎 Cargando {envFileName} desde directorio actual: {envPathLocal}");
}
else
{
    Console.WriteLine($"⚠️ Environment: {environmentName}");
    Console.WriteLine($"⚠️ No se encontró archivo {envFileName} en raíz ni en API.");
}

if (envPathToLoad != null)
{
    DotNetEnv.Env.Load(envPathToLoad);
    Console.WriteLine($"🌎 DB_HOST cargado: {Environment.GetEnvironmentVariable("DB_HOST")}");
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
    .WithOrigins("http://localhost:5173") 
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