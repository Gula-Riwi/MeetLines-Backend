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

// Add FluentValidation localized error handling
builder.Services.AddFluentValidationLocalized();

builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MeetLines API", Version = "v1" });
});

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();
