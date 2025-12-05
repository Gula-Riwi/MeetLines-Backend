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
        
        public async Task SendPasswordChangedNotificationAsync(string toEmail, string userName)
        {
            var subject = "Tu contraseña ha sido cambiada - MeetLines";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>¡Hola {userName}!</h2>
                        <p>Te informamos que tu contraseña ha sido cambiada exitosamente.</p>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #28a745; margin: 20px 0;'>
                            <p style='margin: 0;'><strong>Fecha y hora:</strong> {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                            <p style='margin: 10px 0 0 0;'><strong>Acción:</strong> Cambio de contraseña</p>
                        </div>

                        <p style='color: #666;'>Por seguridad, todas tus sesiones activas han sido cerradas. Necesitarás iniciar sesión nuevamente con tu nueva contraseña.</p>

                        <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                            <p style='margin: 0; color: #856404;'><strong>⚠️ ¿No fuiste tú?</strong></p>
                            <p style='margin: 10px 0 0 0; color: #856404;'>Si no realizaste este cambio, tu cuenta podría estar comprometida. Por favor, contacta con nuestro equipo de soporte inmediatamente.</p>
                        </div>

                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{_frontendUrl}/login' 
                               style='background-color: #28a745; color: white; padding: 14px 28px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                                Iniciar Sesión
                            </a>
                        </div>

                        <hr style='border: 1px solid #eee; margin: 30px 0;'>
                        <p style='color: #999; font-size: 12px;'>Este es un correo automático de seguridad. Si tienes alguna pregunta, contacta con soporte.</p>
                        <p style='color: #999; font-size: 12px;'>Equipo de MeetLines</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailVerifiedNotificationAsync(string toEmail, string userName)
        {
            var subject = "¡Tu cuenta ha sido verificada! - MeetLines";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #4CAF50;'>¡Felicidades {userName}!</h2>
                        <p>Tu correo ha sido verificado correctamente.</p>
                        <p>Ahora tienes acceso completo para crear proyectos y empezar a agendar.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{_frontendUrl}/dashboard' 
                               style='background-color: #2196F3; color: white; padding: 14px 28px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                                Ir al Dashboard
                            </a>
                        </div>
                    </div>
                </body>
                </html>";
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendProjectCreatedNotificationAsync(string toEmail, string userName, string projectName)
        {
            var subject = $"Has creado el proyecto: {projectName} - MeetLines";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>¡Excelente {userName}!</h2>
                        <p>El proyecto <strong>{projectName}</strong> ha sido creado exitosamente.</p>
                        <p>Ya puedes empezar a configurar tus empleados y servicios.</p>
                    </div>
                </body>
                </html>";
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmployeeCredentialsAsync(string toEmail, string name, string username, string password, string area)
        {
            var subject = "Credenciales de acceso empleado - MeetLines";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>Hola {name},</h2>
                        <p>Has sido registrado como empleado en el área de <strong>{area}</strong>.</p>
                        <p>Aquí tienes tus credenciales para acceder al sistema:</p>
                        <ul style='background-color: #f5f5f5; padding: 15px; border-radius: 5px;'>
                            <li><strong>Usuario:</strong> {username}</li>
                            <li><strong>Contraseña temporal:</strong> {password}</li>
                        </ul>
                        <p>Por favor inicia sesión y cambia tu contraseña lo antes posible.</p>
                    </div>
                </body>
                </html>";
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendAppointmentAssignedAsync(string toEmail, string employeeName, string clientName, DateTime date, string time)
        {
            var subject = "Nueva cita asignada - MeetLines";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2196F3;'>Nueva Cita Asignada</h2>
                        <p>Hola {employeeName},</p>
                        <p>Se te ha asignado una nueva cita con el cliente <strong>{clientName}</strong>.</p>
                        <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p><strong>Fecha:</strong> {date:dd/MM/yyyy}</p>
                            <p><strong>Hora:</strong> {time}</p>
                        </div>
                        <p>Por favor asegúrate de estar disponible.</p>
                    </div>
                </body>
                </html>";
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendAppointmentConfirmedAsync(string toEmail, string clientName, string employeeName, DateTime date, string time)
        {
            var subject = "Confirmación de Cita - MeetLines";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #4CAF50;'>¡Cita Confirmada!</h2>
                        <p>Hola {clientName},</p>
                        <p>Tu cita ha sido confirmada exitosamente con <strong>{employeeName}</strong>.</p>
                        <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p><strong>Fecha:</strong> {date:dd/MM/yyyy}</p>
                            <p><strong>Hora:</strong> {time}</p>
                        </div>
                        <p>Te esperamos.</p>
                    </div>
                </body>
                </html>";
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendAppointmentCancelledAsync(string toEmail, string userName, DateTime date, string time, string reason)
        {
            var subject = "Cancelación de Cita - MeetLines";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #F44336;'>Cita Cancelada</h2>
                        <p>Hola {userName},</p>
                        <p>Lamentamos informarte que la cita programada ha sido cancelada.</p>
                        <div style='background-color: #ffebee; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p><strong>Fecha:</strong> {date:dd/MM/yyyy}</p>
                            <p><strong>Hora:</strong> {time}</p>
                            <p><strong>Motivo:</strong> {reason ?? "No especificado"}</p>
                        </div>
                        <p>Si deseas reagendar, por favor visita nuestro sitio web.</p>
                    </div>
                </body>
                </html>";
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