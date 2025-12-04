using MeetLines.Application.IoC;
using MeetLines.Infrastructure.IoC;
using MeetLines.API.Middleware;
using Microsoft.OpenApi.Models;
using DotNetEnv;

// Load .env file FIRST before anything else
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// Register application services
builder.Services.AddApplicationServices();

// Register infrastructure (DB, repositories, services, JWT auth)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
<<<<<<< HEAD
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "MeetLines API", 
        Version = "v1",
        Description = "API de autenticación con Clean Architecture"
    });
    
    // ApiKey para permitir control total sobre el header "Authorization"
=======
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MeetLines API", Version = "v1" });
    // Añadir definición de seguridad Bearer para permitir enviar el token JWT en el header
>>>>>>> aaffe6840ed5454e820fbc480d8b1d28a0b1d287
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticación JWT usando el esquema Bearer.\r\n\r\nIngrese la palabra 'Bearer' seguida de un espacio y su token.\r\n\r\nEjemplo: \"Bearer eyJhbGciOi...\"",
        Name = "Authorization",
<<<<<<< HEAD
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey, 
        Scheme = "Bearer"
=======
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
>>>>>>> aaffe6840ed5454e820fbc480d8b1d28a0b1d287
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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

<<<<<<< HEAD
app.UseAuthentication(); 
=======
// Add Tenant Resolution Middleware BEFORE authentication
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthentication();
>>>>>>> aaffe6840ed5454e820fbc480d8b1d28a0b1d287
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();
