using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MeetLines.Application.Services.Interfaces;

namespace MeetLines.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailTemplateBuilder _templateBuilder;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _frontendUrl;

        public EmailService(IConfiguration configuration, IEmailTemplateBuilder templateBuilder)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _templateBuilder = templateBuilder ?? throw new ArgumentNullException(nameof(templateBuilder));
            
            _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            
            if (!int.TryParse(_configuration["Email:SmtpPort"], out _smtpPort))
            {
                _smtpPort = 587;
            }

            _smtpUser = _configuration["Email:SmtpUser"] ?? throw new ArgumentException("Email:SmtpUser is missing");
            _smtpPassword = _configuration["Email:SmtpPassword"] ?? throw new ArgumentException("Email:SmtpPassword is missing");
            _fromEmail = _configuration["Email:FromEmail"] ?? _smtpUser;
            _fromName = _configuration["Email:FromName"] ?? "MeetLines";
            _frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:3000";
        }

        public async Task SendEmailVerificationAsync(string toEmail, string userName, string verificationToken)
        {
            var verificationUrl = $"{_frontendUrl}/verify-email?token={verificationToken}";
            var subject = "Verifica tu correo electrónico - MeetLines";
            var body = _templateBuilder.BuildEmailVerification(userName, verificationUrl);
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetAsync(string toEmail, string userName, string resetToken)
        {
            var resetUrl = $"{_frontendUrl}/reset-password?token={resetToken}";
            var subject = "Recuperación de contraseña - MeetLines";
            var body = _templateBuilder.BuildPasswordReset(userName, resetUrl);
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "¡Bienvenido a MeetLines!";
            var loginUrl = $"{_frontendUrl}/login";
            var body = _templateBuilder.BuildWelcome(userName, loginUrl);
            await SendEmailAsync(toEmail, subject, body);
        }
        
        public async Task SendPasswordChangedNotificationAsync(string toEmail, string userName)
        {
            var subject = "Tu contraseña ha sido cambiada - MeetLines";
            var loginUrl = $"{_frontendUrl}/login";
            var body = _templateBuilder.BuildPasswordChanged(userName, loginUrl);
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailVerifiedNotificationAsync(string toEmail, string userName)
        {
            var subject = "¡Tu cuenta ha sido verificada! - MeetLines";
            var dashboardUrl = $"{_frontendUrl}/dashboard";
            var body = _templateBuilder.BuildEmailVerified(userName, dashboardUrl);
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendProjectCreatedNotificationAsync(string toEmail, string userName, string projectName)
        {
            var subject = $"Has creado el proyecto: {projectName} - MeetLines";
            var body = _templateBuilder.BuildProjectCreated(userName, projectName);
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmployeeCredentialsAsync(string toEmail, string name, string username, string password, string area)
        {
            var subject = "Credenciales de acceso empleado - MeetLines";
            var body = _templateBuilder.BuildEmployeeCredentials(name, username, password, area);
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendAppointmentAssignedAsync(string toEmail, string employeeName, string clientName, DateTime date, string time, string? senderName = null)
        {
            var subject = "Nueva cita asignada - MeetLines";
            var body = _templateBuilder.BuildAppointmentAssigned(employeeName, clientName, date, time);
            await SendEmailAsync(toEmail, subject, body, senderName);
        }

        public async Task SendAppointmentConfirmedAsync(string toEmail, string clientName, string employeeName, DateTime date, string time, string? senderName = null)
        {
            var subject = "Confirmación de Cita - MeetLines";
            var body = _templateBuilder.BuildAppointmentConfirmed(clientName, employeeName, date, time);
            await SendEmailAsync(toEmail, subject, body, senderName);
        }

        public async Task SendAppointmentCancelledAsync(string toEmail, string userName, DateTime date, string time, string reason, string? senderName = null)
        {
            var subject = "Cancelación de Cita - MeetLines";
            var body = _templateBuilder.BuildAppointmentCancelled(userName, date, time, reason);
            await SendEmailAsync(toEmail, subject, body, senderName);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? fromName = null)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName ?? _fromName, _fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpUser, _smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Log the error (you can inject ILogger here)
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
}