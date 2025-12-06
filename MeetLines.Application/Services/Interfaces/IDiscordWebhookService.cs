using System.Threading.Tasks;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IDiscordWebhookService
    {
        // Autenticación
        Task SendInfoAsync(string title, string description);
        Task SendEmbedAsync(string title, string description, int color);

        Task SendUserRegisteredAsync(string userName, string email, string timezone);
        Task SendUserLoginAsync(string userName, string email, string deviceInfo, string ipAddress);
        Task SendUserLogoutAsync(string userName, string email);
        Task SendEmailVerifiedAsync(string userName, string email);
        
        // Perfil
        Task SendProfileUpdatedAsync(string userName, string email, string changes);
        Task SendPasswordChangedAsync(string userName, string email);
        
        // Suscripciones
        Task SendSubscriptionCreatedAsync(string userName, string email, string plan, decimal price);
        Task SendSubscriptionUpgradedAsync(string userName, string email, string oldPlan, string newPlan);
        Task SendSubscriptionCancelledAsync(string userName, string email, string plan);
        
        // Proyectos
        Task SendProjectCreatedAsync(string userName, string projectName, string projectId);
        Task SendProjectUpdatedAsync(string userName, string projectName, string projectId);
        Task SendProjectDeletedAsync(string userName, string projectName, string projectId);
        
        // Leads
        Task SendLeadCreatedAsync(string projectName, string leadName, string leadEmail, string stage);
        
        // Appointments
        Task SendAppointmentCreatedAsync(string projectName, string appointmentTitle, string scheduledAt);
        
        // Errores del servidor
        Task SendServerErrorAsync(string errorMessage, string stackTrace, string endpoint);
        Task SendCriticalErrorAsync(string errorMessage, string context);
    }
}