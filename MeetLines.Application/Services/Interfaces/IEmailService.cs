using System.Threading.Tasks;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string toEmail, string userName, string verificationToken);
        Task SendPasswordResetAsync(string toEmail, string userName, string resetToken);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
        Task SendPasswordChangedNotificationAsync(string toEmail, string userName);
        
        // New methods
        Task SendEmailVerifiedNotificationAsync(string toEmail, string userName);
        Task SendProjectCreatedNotificationAsync(string toEmail, string userName, string projectName);
        Task SendEmployeeCredentialsAsync(string toEmail, string name, string username, string password, string area);

        // Appointment Notifications
        Task SendAppointmentAssignedAsync(string toEmail, string employeeName, string clientName, DateTime date, string time, string? senderName = null);
        Task SendAppointmentConfirmedAsync(string toEmail, string clientName, string employeeName, DateTime date, string time, string? senderName = null);
        Task SendAppointmentCancelledAsync(string toEmail, string userName, DateTime date, string time, string reason, string? senderName = null);
        
        // Feedback Alerts
        Task SendNegativeFeedbackAlertAsync(string toEmail, string ownerName, string customerName, string customerPhone, int rating, string comment, string projectName);
    }
}