using MeetLines.Application.IoC;
using MeetLines.Infrastructure.IoC;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

// Load .env file from workspace root FIRST before anything else
var envPathRoot = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, ".env");
if (File.Exists(envPathRoot))
{
    DotNetEnv.Env.Load(envPathRoot);
    Console.WriteLine($"🌎 .env cargado desde raíz: {envPathRoot}");
    Console.WriteLine($"🌎 DB_HOST desde .env: {Environment.GetEnvironmentVariable("DB_HOST")}");
}
else
{
    // Fallback: intentar cargar desde la carpeta actual (MeetLines.API)
    var envPathLocal = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (File.Exists(envPathLocal))
    {
        DotNetEnv.Env.Load(envPathLocal);
        Console.WriteLine($"🌎 .env cargado desde API: {envPathLocal}");
        Console.WriteLine($"🌎 DB_HOST desde .env: {Environment.GetEnvironmentVariable("DB_HOST")}");
    }
    else
    {
        Console.WriteLine($"⚠️ No se encontró archivo .env en la raíz ni en MeetLines.API");
    }
}

var builder = WebApplication.CreateBuilder(args);

// Expande variables de entorno en la cadena de conexión
var config = builder.Configuration;
var connectionString = config.GetConnectionString("DefaultConnection");
if (connectionString != null)
{
    var expanded = Environment.ExpandEnvironmentVariables(connectionString);
    builder.Configuration["ConnectionStrings:DefaultConnection"] = expanded;
    Console.WriteLine("📊 Conectando a BD local/configurable");
}

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

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();