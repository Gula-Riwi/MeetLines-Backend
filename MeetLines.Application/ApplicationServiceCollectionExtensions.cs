using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MeetLines.Application.Services;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Application.Validators;
using MeetLines.Application.UseCases.Auth;
using MeetLines.Application.UseCases.HealthCheck;
using MeetLines.Application.UseCases.Projects;
using MeetLines.Application.UseCases.Channels;

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
            // Registrar servicios de aplicación
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IEmployeeService, EmployeeService>();

            // WhatsApp Bot System Services
            services.AddScoped<IProjectBotConfigService, ProjectBotConfigService>();
            services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<ICustomerFeedbackService, CustomerFeedbackService>();
            services.AddScoped<IBotMetricsService, BotMetricsService>();
            services.AddScoped<IAppointmentService, AppointmentService>();

            // Registrar casos de uso de autenticación
            services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
            services.AddScoped<ILoginUserUseCase, LoginUserUseCase>();
            services.AddScoped<IOAuthLoginUseCase, OAuthLoginUseCase>();
            services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
            services.AddScoped<MeetLines.Application.Services.Interfaces.ITransferUseCases, MeetLines.Application.UseCases.Auth.TransferUseCases>();
            services.AddScoped<IVerifyEmailUseCase, VerifyEmailUseCase>();
            services.AddScoped<IForgotPasswordUseCase, ForgotPasswordUseCase>();
            services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();
            services.AddScoped<IResendVerificationEmailUseCase, ResendVerificationEmailUseCase>();
            services.AddScoped<ILogoutUseCase, LogoutUseCase>();
            services.AddScoped<IHealthCheckUseCase, HealthCheckUseCase>();

            // Registrar casos de uso de proyectos
            services.AddScoped<ICreateProjectUseCase, CreateProjectUseCase>();
            services.AddScoped<IGetUserProjectsUseCase, GetUserProjectsUseCase>();
            services.AddScoped<IGetProjectByIdUseCase, GetProjectByIdUseCase>();
            services.AddScoped<IUpdateProjectUseCase, UpdateProjectUseCase>();
            services.AddScoped<IDeleteProjectUseCase, DeleteProjectUseCase>();
            services.AddScoped<IConfigureWhatsappUseCase, ConfigureWhatsappUseCase>();
            services.AddScoped<IGetPublicProjectsUseCase, GetPublicProjectsUseCase>();
            services.AddScoped<IGetPublicProjectEmployeesUseCase, GetPublicProjectEmployeesUseCase>();

            // Channels Use Cases
            services.AddScoped<ICreateChannelUseCase, CreateChannelUseCase>();
            services.AddScoped<IGetProjectChannelsUseCase, GetProjectChannelsUseCase>();
            services.AddScoped<IDeleteChannelUseCase, DeleteChannelUseCase>();
            services.AddScoped<IGetPublicProjectChannelsUseCase, GetPublicProjectChannelsUseCase>();
            services.AddScoped<IUpdateChannelUseCase, UpdateChannelUseCase>();

            // Registrar validadores de FluentValidation
            services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

            return services;
        }
    }
}