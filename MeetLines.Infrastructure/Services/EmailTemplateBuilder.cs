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
            return $@"<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <title>{title}</title>
    <style>
        /* Reseteo básico */
        body {{ margin: 0; padding: 0; background-color: {BackgroundColor}; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }}
        table {{ border-collapse: collapse; }}
        
        /* Estilos generales */
        .container {{ 
            max-width: 600px; 
            margin: 0 auto; 
            background-color: {CardColor}; 
            color: {TextColor};
            border-radius: 8px;
            overflow: hidden;
        }}
        .header {{ 
            padding: 30px; 
            text-align: center; 
            background-color: {CardColor};
            border-bottom: 1px solid #233554;
        }}
        .logo {{ 
            font-size: 28px; 
            font-weight: bold; 
            color: {HeadingColor}; 
        }}
        .content {{ 
            padding: 40px 30px; 
            line-height: 1.6;
        }}
        .footer {{ 
            background-color: #0f1c33; 
            padding: 20px; 
            text-align: center; 
            font-size: 12px; 
            color: #8892b0;
        }}
        a {{ color: {PrimaryColor}; }}
        
        /* Media Query para móviles */
        @media only screen and (max-width: 600px) {{
            .container {{ width: 100% !important; border-radius: 0 !important; }}
            .content {{ padding: 20px !important; }}
        }}
    </style>
</head>
<body style='margin: 0; padding: 0; background-color: {BackgroundColor};'>
    <center>
        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: {BackgroundColor}; height: 100vh;'>
            <tr>
                <td align='center' valign='top' style='padding: 40px 10px;'>
                    <!-- Contenedor Principal -->
                    <table border='0' cellpadding='0' cellspacing='0' width='600' class='container' style='background-color: {CardColor}; border-radius: 8px;'>
                        <!-- Header -->
                        <tr>
                            <td class='header' align='center'>
                                <div class='logo'>MeetLines</div>
                            </td>
                        </tr>
                        
                        <!-- Contenido -->
                        <tr>
                            <td class='content' align='left' style='color: {TextColor}; font-size: 16px;'>
                                {content}
                            </td>
                        </tr>
                        
                        <!-- Footer -->
                        <tr>
                            <td class='footer' align='center'>
                                © {DateTime.Now.Year} MeetLines. Todos los derechos reservados.<br>
                                Este es un email automático.
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </center>
</body>
</html>";
        }

        private string BuildButton(string text, string url)
        {
            // TÉCNICA DE BORDES:
            // Usamos bordes sólidos del mismo color que el fondo para simular padding.
            // Esto funciona en Outlook, Gmail, Apple Mail y todos los clientes móviles.
            return $@"
                <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                    <tr>
                        <td align='center' style='padding: 30px 0;'>
                            <a href='{url}' target='_blank' style='
                                background-color: {PrimaryColor}; 
                                color: #ffffff; 
                                font-family: sans-serif; 
                                font-size: 16px; 
                                font-weight: bold; 
                                text-decoration: none; 
                                display: inline-block; 
                                border-radius: 4px; 
                                border-top: 14px solid {PrimaryColor}; 
                                border-bottom: 14px solid {PrimaryColor}; 
                                border-left: 30px solid {PrimaryColor}; 
                                border-right: 30px solid {PrimaryColor};
                                box-sizing: border-box;'>
                                {text}
                            </a>
                        </td>
                    </tr>
                </table>";
        }

        private string BuildInfoBox(string content, string borderColor = AccentColor)
        {
            return $@"
                <div style='background-color: rgba(255, 255, 255, 0.05); padding: 15px; border-left: 4px solid {borderColor}; margin: 20px 0; border-radius: 4px;'>
                    {content}
                </div>";
        }



        public string BuildEmailVerification(string userName, string verificationUrl)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin: 0 0 20px 0;'>¡Hola {userName}!</h2>
                <p>Gracias por registrarte en MeetLines. Por favor verifica tu correo electrónico.</p>
                
                {BuildButton("Verificar Email", verificationUrl)}
                
                ";

            return BuildBaseHtml(content, "Verifica tu correo");
        }

        public string BuildPasswordReset(string userName, string resetUrl)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin: 0 0 20px 0;'>Recuperar Contraseña</h2>
                <p>Hola {userName}, recibimos una solicitud para restablecer tu contraseña.</p>
                
                {BuildButton("Restablecer Contraseña", resetUrl)}
                
                <p style='color: #8892b0; font-size: 14px;'>El enlace expira en 1 hora.</p>
                
                ";

            return BuildBaseHtml(content, "Recuperar contraseña");
        }

        public string BuildWelcome(string userName, string loginUrl)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin: 0 0 20px 0;'>¡Bienvenido a MeetLines!</h2>
                <p>Tu cuenta ha sido verificada exitosamente.</p>
                
                {BuildButton("Ir a mi Dashboard", loginUrl)}
                
                ";

            return BuildBaseHtml(content, "Bienvenido");
        }

        public string BuildPasswordChanged(string userName, string loginUrl)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin: 0 0 20px 0;'>Contraseña Actualizada</h2>
                <p>Hola {userName}, tu contraseña ha sido cambiada.</p>
                
                {BuildInfoBox($"<strong style='color:{HeadingColor}'>Fecha:</strong> {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm} UTC", "#28a745")}
                
                {BuildButton("Iniciar Sesión", loginUrl)}
                
                ";

            return BuildBaseHtml(content, "Contraseña cambiada");
        }

        public string BuildEmailVerified(string userName, string dashboardUrl)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin: 0 0 20px 0;'>¡Cuenta Verificada!</h2>
                <p>Hola {userName}, tu correo ha sido confirmado.</p>
                
                {BuildButton("Comenzar Ahora", dashboardUrl)}
                
                ";

            return BuildBaseHtml(content, "Cuenta verificada");
        }

        public string BuildProjectCreated(string userName, string projectName)
        {
            var content = $@"
                <h2 style='color: {AccentColor}; margin: 0 0 20px 0;'>Nuevo Proyecto</h2>
                <p>El proyecto <strong>{projectName}</strong> ha sido creado.</p>
                {BuildInfoBox("Configura tus servicios para empezar.", AccentColor)}";

            return BuildBaseHtml(content, "Proyecto Creado");
        }

        public string BuildEmployeeCredentials(string name, string username, string password, string area)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin: 0 0 20px 0;'>Bienvenido, {name}</h2>
                <p>Área: <strong style='color: {AccentColor};'>{area}</strong></p>
                
                {BuildInfoBox($@"
                    <div style='color: {HeadingColor};'>
                        <strong>Usuario:</strong> {username}<br>
                        <strong>Contraseña:</strong> {password}
                    </div>
                ", PrimaryColor)}
                
                <p>Por favor cambia tu contraseña al entrar.</p>";

            return BuildBaseHtml(content, "Credenciales");
        }

        public string BuildAppointmentAssigned(string employeeName, string clientName, DateTime date, string time)
        {
            var content = $@"
                <h2 style='color: {HeadingColor}; margin: 0 0 20px 0;'>Nueva Cita Asignada</h2>
                <p>Hola {employeeName}, tienes una nueva cita.</p>
                
                {BuildInfoBox($@"
                    <strong>Cliente:</strong> {clientName}<br>
                    <strong>Fecha:</strong> {date:dd/MM/yyyy} - {time}
                ", AccentColor)}";

            return BuildBaseHtml(content, "Nueva Cita");
        }

        public string BuildAppointmentConfirmed(string clientName, string employeeName, DateTime date, string time)
        {
            var content = $@"
                <h2 style='color: {AccentColor}; margin: 0 0 20px 0;'>¡Cita Confirmada!</h2>
                <p>Hola {clientName}, tu cita está lista.</p>
                
                {BuildInfoBox($@"
                    <strong>Profesional:</strong> {employeeName}<br>
                    <strong>Fecha:</strong> {date:dd/MM/yyyy} - {time}
                ", "#28a745")}";

            return BuildBaseHtml(content, "Cita Confirmada");
        }

        public string BuildAppointmentCancelled(string userName, DateTime date, string time, string reason)
        {
            var content = $@"
                <h2 style='color: #ff6b6b; margin: 0 0 20px 0;'>Cita Cancelada</h2>
                <p>Hola {userName}, tu cita ha sido cancelada.</p>
                
                {BuildInfoBox($@"
                    <strong>Fecha:</strong> {date:dd/MM/yyyy} a las {time}<br>
                    <strong>Motivo:</strong> {reason ?? "No especificado"}
                ", "#ff6b6b")}";

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