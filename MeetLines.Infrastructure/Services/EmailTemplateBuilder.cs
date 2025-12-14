using System;
using MeetLines.Application.Services.Interfaces;

namespace MeetLines.Infrastructure.Services
{
    public class EmailTemplateBuilder : IEmailTemplateBuilder
    {
        private const string PrimaryColor = "#6366f1"; // Indigo-500
        private const string BackgroundColor = "#0a192f"; // Dark Blue
        private const string CardColor = "#112240"; // Light Navy
        private const string TextColor = "#ccd6f6"; // Lightest Slate
        private const string HeadingColor = "#ffffff"; // White
        private const string AccentColor = "#64ffda"; // Teal/Cyan accent

        private string BuildBaseHtml(string content, string title)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>{title}</title>
                </head>
                <body style='margin: 0; padding: 0; font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; background-color: {BackgroundColor}; color: {TextColor};'>
                    <table role='presentation' border='0' cellpadding='0' cellspacing='0' width='100%'>
                        <tr>
                            <td align='center' style='padding: 40px 0;'>
                                <table role='presentation' border='0' cellpadding='0' cellspacing='0' width='600' style='border-collapse: collapse; max-width: 600px; width: 100%;'>
                                    <!-- Header -->
                                    <tr>
                                        <td align='center' style='padding: 0 0 20px 0;'>
                                            <h1 style='color: {HeadingColor}; margin: 0; font-size: 28px; font-weight: bold; letter-spacing: 1px;'>MeetLines</h1>
                                        </td>
                                    </tr>
                                    
                                    <!-- Content -->
                                    <tr>
                                        <td style='background-color: {CardColor}; padding: 40px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);'>
                                            {content}
                                        </td>
                                    </tr>

                                    <!-- Footer -->
                                    <tr>
                                        <td align='center' style='padding: 30px 0; color: #8892b0; font-size: 12px;'>
                                            <p style='margin: 0;'>&copy; {DateTime.Now.Year} MeetLines. Todos los derechos reservados.</p>
                                            <p style='margin: 10px 0 0 0;'>
                                                <a href='#' style='color: {PrimaryColor}; text-decoration: none;'>Términos</a> | 
                                                <a href='#' style='color: {PrimaryColor}; text-decoration: none;'>Privacidad</a>
                                            </p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";
        }

        private string BuildButton(string text, string url)
        {
            return $@"
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{url}' style='background-color: {PrimaryColor}; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold; box-shadow: 0 4px 6px rgba(99, 102, 241, 0.25);'>
                        {text}
                    </a>
                </div>";
        }

        private string BuildInfoBox(string content, string borderColor = AccentColor)
        {
            return $@"
                <div style='background-color: rgba(255, 255, 255, 0.05); padding: 20px; border-left: 4px solid {borderColor}; margin: 25px 0; border-radius: 4px;'>
                    {content}
                </div>";
        }

        public string BuildEmailVerification(string userName, string verificationUrl)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin-top: 0;'>¡Hola {userName}!</h2>
                <p>Gracias por registrarte en MeetLines. Para completar la configuración de tu cuenta y acceder a todas las funciones, por favor verifica tu correo electrónico.</p>
                
                {BuildButton("Verificar Email", verificationUrl)}
                
                <p style='font-size: 14px; color: #8892b0;'>O copia y pega este enlace:</p>
                <p style='font-size: 12px; color: {PrimaryColor}; word-break: break-all;'>{verificationUrl}</p>
                <p style='font-size: 14px; margin-top: 20px;'>Este enlace expirará en 24 horas.</p>";

            return BuildBaseHtml(content, "Verifica tu correo");
        }

        public string BuildPasswordReset(string userName, string resetUrl)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin-top: 0;'>Recuperación de Contraseña</h2>
                <p>Hola {userName}, hemos recibido una solicitud para restablecer la contraseña de tu cuenta en MeetLines.</p>
                
                {BuildButton("Restablecer Contraseña", resetUrl)}
                
                <p style='font-size: 14px; margin-top: 20px;'>Si no solicitaste este cambio, puedes ignorar este correo de forma segura.</p>
                <p style='font-size: 14px; color: #8892b0;'>El enlace expirará en 1 hora.</p>";

            return BuildBaseHtml(content, "Recuperar contraseña");
        }

        public string BuildWelcome(string userName, string loginUrl)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin-top: 0;'>¡Bienvenido a MeetLines!</h2>
                <p>¡Hola {userName}! Tu cuenta ha sido verificada exitosamente.</p>
                <p>Estamos emocionados de tenerte con nosotros. Ahora puedes gestionar tus citas y proyectos de manera eficiente con nuestra plataforma.</p>
                
                {BuildButton("Ir a mi Dashboard", loginUrl)}
                
                <p>Si tienes alguna pregunta, nuestro equipo de soporte está aquí para ayudarte.</p>";

            return BuildBaseHtml(content, "Bienvenido");
        }

        public string BuildPasswordChanged(string userName, string loginUrl)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin-top: 0;'>Contraseña Actualizada</h2>
                <p>Hola {userName},</p>
                <p>Te informamos que tu contraseña ha sido cambiada exitosamente.</p>
                
                {BuildInfoBox($@"
                    <p style='margin: 0;'><strong>Fecha:</strong> {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    <p style='margin: 10px 0 0 0;'><strong>Acción:</strong> Cambio de contraseña</p>
                ", "#28a745")}

                <p>Por seguridad, se han cerrado todas las sesiones activas.</p>
                
                {BuildButton("Iniciar Sesión", loginUrl)}

                <div style='margin-top: 30px; border-top: 1px solid rgba(255,255,255,0.1); padding-top: 20px;'>
                    <p style='color: #ff6b6b; font-size: 14px; margin: 0;'><strong>¿No fuiste tú?</strong></p>
                    <p style='font-size: 14px; margin-top: 5px;'>Si no realizaste este cambio, contacta a soporte inmediatamente.</p>
                </div>";

            return BuildBaseHtml(content, "Contraseña cambiada");
        }

        public string BuildEmailVerified(string userName, string dashboardUrl)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin-top: 0;'>¡Cuenta Verificada!</h2>
                <p>Hola {userName},</p>
                <p>Tu correo electrónico ha sido confirmado correctamente. Ya tienes acceso total a la plataforma.</p>
                
                {BuildButton("Comenzar Ahora", dashboardUrl)}
                
                <p>¡Disfruta de la experiencia MeetLines!</p>";

            return BuildBaseHtml(content, "Cuenta verificada");
        }

        public string BuildProjectCreated(string userName, string projectName)
        {
            var content = $@"
                <h2 style='color: {AccentColor}; margin-top: 0;'>Nuevo Proyecto Creado</h2>
                <p>¡Excelente trabajo, {userName}!</p>
                <p>El proyecto <strong>{projectName}</strong> ha sido creado y está listo para configurarse.</p>
                
                {BuildInfoBox("Configura tus servicios y empleados para empezar a recibir citas.", AccentColor)}
                
                <p>¡Mucho éxito con tu nuevo proyecto!</p>";

            return BuildBaseHtml(content, "Proyecto Creado");
        }

        public string BuildEmployeeCredentials(string name, string username, string password, string area)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin-top: 0;'>Bienvenido al equipo, {name}</h2>
                <p>Has sido registrado como empleado en el área: <strong style='color: {AccentColor};'>{area}</strong>.</p>
                <p>A continuación encontrarás tus credenciales de acceso temporal:</p>
                
                {BuildInfoBox($@"
                    <ul style='list-style: none; padding: 0; margin: 0;'>
                        <li style='margin-bottom: 10px;'><strong>Usuario:</strong> {username}</li>
                        <li><strong>Contraseña:</strong> {password}</li>
                    </ul>
                ", PrimaryColor)}
                
                <p>Por motivos de seguridad, te recomendamos cambiar tu contraseña al iniciar sesión por primera vez.</p>";

            return BuildBaseHtml(content, "Credenciales de Acceso");
        }

        public string BuildAppointmentAssigned(string employeeName, string clientName, DateTime date, string time)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin-top: 0;'>Nueva Cita Asignada</h2>
                <p>Hola {employeeName},</p>
                <p>Se te ha asignado una nueva cita.</p>
                
                {BuildInfoBox($@"
                    <table style='width: 100%; color: {TextColor};'>
                        <tr>
                            <td style='padding: 5px 0;'><strong>Cliente:</strong></td>
                            <td style='text-align: right;'>{clientName}</td>
                        </tr>
                        <tr>
                            <td style='padding: 5px 0;'><strong>Fecha:</strong></td>
                            <td style='text-align: right;'>{date:dd/MM/yyyy}</td>
                        </tr>
                        <tr>
                            <td style='padding: 5px 0;'><strong>Hora:</strong></td>
                            <td style='text-align: right;'>{time}</td>
                        </tr>
                    </table>
                ", AccentColor)}
                
                <p>Por favor, asegúrate de estar preparado.</p>";

            return BuildBaseHtml(content, "Nueva Cita");
        }

        public string BuildAppointmentConfirmed(string clientName, string employeeName, DateTime date, string time)
        {
            var content = $@"
                <h2 style='color: {AccentColor}; margin-top: 0;'>¡Cita Confirmada!</h2>
                <p>Hola {clientName},</p>
                <p>Tu cita ha sido confirmada exitosamente.</p>
                
                {BuildInfoBox($@"
                    <table style='width: 100%; color: {TextColor};'>
                        <tr>
                            <td style='padding: 5px 0;'><strong>Profesional:</strong></td>
                            <td style='text-align: right;'>{employeeName}</td>
                        </tr>
                        <tr>
                            <td style='padding: 5px 0;'><strong>Fecha:</strong></td>
                            <td style='text-align: right;'>{date:dd/MM/yyyy}</td>
                        </tr>
                        <tr>
                            <td style='padding: 5px 0;'><strong>Hora:</strong></td>
                            <td style='text-align: right;'>{time}</td>
                        </tr>
                    </table>
                ", "#28a745")}
                
                <p>¡Te esperamos!</p>";

            return BuildBaseHtml(content, "Cita Confirmada");
        }

        public string BuildAppointmentCancelled(string userName, DateTime date, string time, string reason)
        {
            var content = $@"
                <h2 style='color: #ff6b6b; margin-top: 0;'>Cita Cancelada</h2>
                <p>Hola {userName},</p>
                <p>Lamentamos informarte que tu cita ha sido cancelada.</p>
                
                {BuildInfoBox($@"
                    <p style='margin: 0 0 10px 0;'><strong>Detalles de la cita:</strong></p>
                    <p style='margin: 0;'>{date:dd/MM/yyyy} a las {time}</p>
                    <hr style='border: 0; border-top: 1px solid rgba(255,255,255,0.1); margin: 15px 0;'>
                    <p style='margin: 0 0 5px 0;'><strong>Motivo:</strong></p>
                    <p style='margin: 0; color: #ff6b6b;'>{reason ?? "No especificado"}</p>
                ", "#ff6b6b")}
                
                <p>Si deseas reagendar, por favor visita nuestra plataforma.</p>";

            return BuildBaseHtml(content, "Cita Cancelada");
        }

        public string BuildNegativeFeedbackAlert(string ownerName, string customerName, string customerPhone, int rating, string comment, string projectName)
        {
            var content = $@"
                <h2 style='color: #ff6b6b; margin-top: 0;'>🚨 Alerta de Calificación Baja</h2>
                <p>Hola {ownerName},</p>
                <p>Has recibido una calificación negativa en tu proyecto <strong>{projectName}</strong>. Se recomienda revisar el caso.</p>
                
                {BuildInfoBox($@"
                    <p style='margin: 0 0 5px 0;'><strong>Cliente:</strong> {customerName}</p>
                    <p style='margin: 0 0 5px 0;'><strong>Teléfono:</strong> <a href='https://wa.me/{customerPhone?.Replace("+", "").Trim()}' style='color: #ff6b6b; text-decoration: none;'>{customerPhone} 📲</a></p>
                    <p style='margin: 0 0 15px 0;'><strong>Calificación:</strong> <span style='font-size: 1.2em;'>{new string('⭐', rating)}</span> ({rating}/5)</p>
                    
                    <p style='margin: 0 0 5px 0;'><strong>Comentario:</strong></p>
                    <blockquote style='background: rgba(0,0,0,0.2); padding: 10px; border-radius: 4px; border-left: 2px solid #ff6b6b; margin: 0; font-style: italic;'>
                        ""{comment ?? "Sin comentario"}""
                    </blockquote>
                ", "#ff6b6b")}
                
                <p>Te sugerimos contactar al cliente para manejar la situación.</p>";

            return BuildBaseHtml(content, "Alerta Feedback Negativo");
        }
    }
}
