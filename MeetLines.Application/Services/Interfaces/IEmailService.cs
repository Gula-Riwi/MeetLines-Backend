using System.Threading.Tasks;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string toEmail, string userName, string verificationToken);
        Task SendPasswordResetAsync(string toEmail, string userName, string resetToken);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
    }
}