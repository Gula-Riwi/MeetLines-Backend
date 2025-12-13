using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MeetLines.Application.Services;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Application.Validators;
using MeetLines.Application.UseCases.Auth;
using MeetLines.Application.UseCases.HealthCheck;
using MeetLines.Application.UseCases.Projects;
using MeetLines.Application.UseCases.Channels;
using MeetLines.Application.UseCases.TwoFactor;
using MeetLines.Application.UseCases.Projects.Interfaces;
using MeetLines.Application.UseCases.Dashboard;

using MeetLines.Application.UseCases.Services;
using MeetLines.Application.UseCases.Services.Interfaces;

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
            services.AddScoped<IMercadoPagoService, MercadoPagoService>();

            // WhatsApp Bot System Services
            services.AddScoped<IProjectBotConfigService, ProjectBotConfigService>();
            services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<ICustomerFeedbackService, CustomerFeedbackService>();
            services.AddScoped<IBotMetricsService, BotMetricsService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<INotificationService, NotificationService>();

            // Registrar casos de uso de autenticación
            services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
            // services.AddScoped<IChannelUseCases, ChannelUseCases>(); // Removed as it uses individual Use Cases
            // services.AddScoped<IAppointmentUseCases, AppointmentUseCases>(); // Removed as it uses Service pattern
            services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
            services.AddScoped<MeetLines.Application.Services.Interfaces.ITransferUseCases, MeetLines.Application.UseCases.Auth.TransferUseCases>();
            services.AddScoped<IVerifyEmailUseCase, VerifyEmailUseCase>();
            services.AddScoped<IForgotPasswordUseCase, ForgotPasswordUseCase>();
            services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();
            services.AddScoped<IResendVerificationEmailUseCase, ResendVerificationEmailUseCase>();
            services.AddScoped<ILogoutUseCase, LogoutUseCase>();
            services.AddScoped<MeetLines.Application.UseCases.Auth.ClientAuth.Interfaces.IClientAuthUseCase, MeetLines.Application.UseCases.Auth.ClientAuth.ClientAuthUseCase>();
            services.AddScoped<IHealthCheckUseCase, HealthCheckUseCase>();
            
            // 2FA Use Cases
            services.AddScoped<Enable2FAUseCase>();
            services.AddScoped<Disable2FAUseCase>();
            services.AddScoped<Verify2FACodeUseCase>();
            services.AddScoped<Validate2FALoginUseCase>();

            // Registrar casos de uso de proyectos
            services.AddScoped<ICreateProjectUseCase, CreateProjectUseCase>();
            services.AddScoped<IGetUserProjectsUseCase, GetUserProjectsUseCase>();
            services.AddScoped<IGetProjectByIdUseCase, GetProjectByIdUseCase>();
            services.AddScoped<IUpdateProjectUseCase, UpdateProjectUseCase>();
            services.AddScoped<IDeleteProjectUseCase, DeleteProjectUseCase>();
            services.AddScoped<IConfigureWhatsappUseCase, ConfigureWhatsappUseCase>();
            services.AddScoped<IGetPublicProjectsUseCase, GetPublicProjectsUseCase>();
            services.AddScoped<IGetPublicProjectEmployeesUseCase, GetPublicProjectEmployeesUseCase>();
            services.AddScoped<IUploadProjectPhotoUseCase, UploadProjectPhotoUseCase>();
            services.AddScoped<IGetProjectPhotosUseCase, GetProjectPhotosUseCase>();
            services.AddScoped<IDeleteProjectPhotoUseCase, DeleteProjectPhotoUseCase>();
            services.AddScoped<IConfigureTelegramUseCase, ConfigureTelegramUseCase>();
            
            // Dashboard Use Cases
            services.AddScoped<GetDashboardStatsUseCase>();
            services.AddScoped<GetDashboardTasksUseCase>();

            // Channels Use Cases
            services.AddScoped<ICreateChannelUseCase, CreateChannelUseCase>();
            services.AddScoped<IGetProjectChannelsUseCase, GetProjectChannelsUseCase>();
            services.AddScoped<IDeleteChannelUseCase, DeleteChannelUseCase>();
            services.AddScoped<IGetPublicProjectChannelsUseCase, GetPublicProjectChannelsUseCase>();
            services.AddScoped<IUpdateChannelUseCase, UpdateChannelUseCase>();

            // Services Use Cases (CRUD)
            services.AddScoped<ICreateServiceUseCase, CreateServiceUseCase>();
            services.AddScoped<IUpdateServiceUseCase, UpdateServiceUseCase>();
            services.AddScoped<IDeleteServiceUseCase, DeleteServiceUseCase>();
            services.AddScoped<IGetProjectServicesUseCase, GetProjectServicesUseCase>();
            services.AddScoped<IGetServiceByIdUseCase, GetServiceByIdUseCase>();

            // Registrar validadores de FluentValidation
            services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
            
            return services;
        }
    }
}
