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
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _frontendUrl;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
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
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>¡Hola {userName}!</h2>
                        <p>Gracias por registrarte en MeetLines. Para completar tu registro, por favor verifica tu correo electrónico.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{verificationUrl}' 
                               style='background-color: #4CAF50; color: white; padding: 14px 28px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                                Verificar Email
                            </a>
                        </div>
                        <p style='color: #666; font-size: 14px;'>O copia y pega este enlace en tu navegador:</p>
                        <p style='color: #999; font-size: 12px; word-break: break-all;'>{verificationUrl}</p>
                        <p style='color: #666; font-size: 14px;'>Este enlace expirará en 24 horas.</p>
                        <hr style='border: 1px solid #eee; margin: 30px 0;'>
                        <p style='color: #999; font-size: 12px;'>Si no creaste esta cuenta, puedes ignorar este correo.</p>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetAsync(string toEmail, string userName, string resetToken)
        {
            var resetUrl = $"{_frontendUrl}/reset-password?token={resetToken}";
            
            var subject = "Recuperación de contraseña - MeetLines";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>¡Hola {userName}!</h2>
                        <p>Recibimos una solicitud para restablecer tu contraseña.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetUrl}' 
                               style='background-color: #2196F3; color: white; padding: 14px 28px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                                Restablecer Contraseña
                            </a>
                        </div>
                        <p style='color: #666; font-size: 14px;'>O copia y pega este enlace en tu navegador:</p>
                        <p style='color: #999; font-size: 12px; word-break: break-all;'>{resetUrl}</p>
                        <p style='color: #666; font-size: 14px;'>Este enlace expirará en 1 hora.</p>
                        <hr style='border: 1px solid #eee; margin: 30px 0;'>
                        <p style='color: #999; font-size: 12px;'>Si no solicitaste este cambio, puedes ignorar este correo.</p>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "¡Bienvenido a MeetLines!";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>¡Bienvenido a MeetLines, {userName}!</h2>
                        <p>Tu cuenta ha sido verificada exitosamente.</p>
                        <p>Ahora puedes empezar a usar todas las funcionalidades de nuestra plataforma.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{_frontendUrl}/login' 
                               style='background-color: #4CAF50; color: white; padding: 14px 28px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                                Iniciar Sesión
                            </a>
                        </div>
                        <p style='color: #666; font-size: 14px;'>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                        <hr style='border: 1px solid #eee; margin: 30px 0;'>
                        <p style='color: #999; font-size: 12px;'>Equipo de MeetLines</p>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
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