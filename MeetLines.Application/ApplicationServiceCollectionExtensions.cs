using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MeetLines.Application.Services;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Application.Validators;
using MeetLines.Application.UseCases.Auth;

namespace MeetLines.Application.IoC
{
    /// <summary>
    /// Extensión de configuración de servicios de aplicación.
    /// Implementa el patrón de inyección de dependencias para DDD hexagonal.
    /// Registra puertos (interfaces), adaptadores (implementaciones) y validadores.
    /// </summary>
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar servicio de aplicación (mantener por compatibilidad hacia atrás)
            services.AddScoped<IAuthService, AuthService>();

            // Registrar casos de uso (puertos de entrada / adaptadores primarios)
            services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
            services.AddScoped<ILoginUserUseCase, LoginUserUseCase>();
            services.AddScoped<IOAuthLoginUseCase, OAuthLoginUseCase>();
            services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
            services.AddScoped<IVerifyEmailUseCase, VerifyEmailUseCase>();
            services.AddScoped<IForgotPasswordUseCase, ForgotPasswordUseCase>();
            services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();
            services.AddScoped<IResendVerificationEmailUseCase, ResendVerificationEmailUseCase>();
            services.AddScoped<ILogoutUseCase, LogoutUseCase>();

            // Registrar validadores de FluentValidation
            services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

            return services;
        }
    }
}