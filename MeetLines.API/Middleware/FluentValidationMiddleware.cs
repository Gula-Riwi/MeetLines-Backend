using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace MeetLines.API.Middleware
{
    /// <summary>
    /// Configurador de validación de modelos para FluentValidation.
    /// Valida automáticamente todos los DTOs usando sus validadores registrados.
    /// Implementa el patrón de puertos y adaptadores de DDD hexagonal.
    /// </summary>
    public static class FluentValidationExtensions
    {
        public static IServiceCollection AddFluentValidationLocalized(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(modelState => modelState.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(modelError => modelError.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        );

                    return new BadRequestObjectResult(new
                    {
                        success = false,
                        message = "Validación fallida",
                        errors = errors
                    });
                };
            });

            return services;
        }
    }
}
