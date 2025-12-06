using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MeetLines.Application.Services.Interfaces;

namespace MeetLines.Application.Services
{
    public class DiscordWebhookService : IDiscordWebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;
        private readonly bool _isEnabled;

        public DiscordWebhookService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _webhookUrl = configuration["Discord:WebhookUrl"] ?? "";
            _isEnabled = !string.IsNullOrEmpty(_webhookUrl);
        }

        // ===== MÉTODOS GENÉRICOS (Necesarios para que compile AuthService) =====

        public async Task SendInfoAsync(string title, string description)
        {
            var embed = new
            {
                title = title,
                description = description,
                color = 3447003, // Azul
                timestamp = DateTimeOffset.UtcNow
            };
            await SendEmbedAsync(embed);
        }

        // Sobrecarga pública para permitir llamadas directas con título y color
        public async Task SendEmbedAsync(string title, string description, int color)
        {
            var embed = new
            {
                title = title,
                description = description,
                color = color,
                timestamp = DateTimeOffset.UtcNow
            };
            await SendEmbedAsync(embed);
        }

        // ===== AUTENTICACIÓN =====
        
        public async Task SendUserRegisteredAsync(string userName, string email, string timezone)
        {
            var embed = new
            {
                title = "🆕 Nuevo Usuario Registrado",
                description = $"Un nuevo usuario se ha registrado en el sistema",
                color = 5763719, // Verde
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📧 Email", value = email, inline = true },
                    new { name = "🌍 Timezone", value = timezone, inline = true }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        public async Task SendUserLoginAsync(string userName, string email, string deviceInfo, string ipAddress)
        {
            var embed = new
            {
                title = "🔓 Usuario Inició Sesión",
                description = $"Usuario ha iniciado sesión",
                color = 3447003, // Azul
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📧 Email", value = email, inline = true },
                    new { name = "📱 Dispositivo", value = deviceInfo ?? "Desconocido", inline = true },
                    new { name = "🌐 IP", value = ipAddress ?? "Desconocida", inline = true }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        public async Task SendUserLogoutAsync(string userName, string email)
        {
            var embed = new
            {
                title = "🔒 Usuario Cerró Sesión",
                description = $"Usuario ha cerrado sesión",
                color = 10070709, // Gris
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📧 Email", value = email, inline = true }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        public async Task SendEmailVerifiedAsync(string userName, string email)
        {
            var embed = new
            {
                title = "✅ Email Verificado",
                description = $"Usuario ha verificado su email",
                color = 5763719, // Verde
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📧 Email", value = email, inline = true }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        // ===== PERFIL =====

        public async Task SendProfileUpdatedAsync(string userName, string email, string changes)
        {
            var embed = new
            {
                title = "✏️ Perfil Actualizado",
                description = $"Usuario ha actualizado su perfil",
                color = 16776960, // Amarillo
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📧 Email", value = email, inline = true },
                    new { name = "📝 Cambios", value = changes, inline = false }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        public async Task SendPasswordChangedAsync(string userName, string email)
        {
            var embed = new
            {
                title = "🔐 Contraseña Cambiada",
                description = $"Usuario ha cambiado su contraseña",
                color = 16744272, // Naranja
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📧 Email", value = email, inline = true },
                    new { name = "🔒 Seguridad", value = "Todas las sesiones fueron cerradas", inline = false }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        // ===== SUSCRIPCIONES =====

        public async Task SendSubscriptionCreatedAsync(string userName, string email, string plan, decimal price)
        {
            var embed = new
            {
                title = "💳 Nueva Suscripción",
                description = $"Usuario obtuvo un plan",
                color = 5763719, // Verde
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📧 Email", value = email, inline = true },
                    new { name = "📦 Plan", value = plan, inline = true },
                    new { name = "💰 Precio", value = $"${price}", inline = true }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        public async Task SendSubscriptionUpgradedAsync(string userName, string email, string oldPlan, string newPlan)
        {
            var embed = new
            {
                title = "⬆️ Upgrade de Plan",
                description = $"Usuario mejoró su suscripción",
                color = 3066993, // Verde oscuro
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📧 Email", value = email, inline = true },
                    new { name = "📦 Plan Anterior", value = oldPlan, inline = true },
                    new { name = "🎁 Plan Nuevo", value = newPlan, inline = true }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        public async Task SendSubscriptionCancelledAsync(string userName, string email, string plan)
        {
            var embed = new
            {
                title = "❌ Suscripción Cancelada",
                description = $"Usuario canceló su suscripción",
                color = 15158332, // Rojo
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📧 Email", value = email, inline = true },
                    new { name = "📦 Plan", value = plan, inline = true }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        // ===== PROYECTOS =====

        public async Task SendProjectCreatedAsync(string userName, string projectName, string projectId)
        {
            var embed = new
            {
                title = "🚀 Nuevo Proyecto",
                description = $"Usuario creó un nuevo proyecto",
                color = 3447003, // Azul
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📁 Proyecto", value = projectName, inline = true },
                    new { name = "🆔 ID", value = projectId, inline = false }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        public async Task SendProjectUpdatedAsync(string userName, string projectName, string projectId)
        {
            var embed = new
            {
                title = "✏️ Proyecto Actualizado",
                description = $"Usuario actualizó un proyecto",
                color = 16776960, // Amarillo
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📁 Proyecto", value = projectName, inline = true },
                    new { name = "🆔 ID", value = projectId, inline = false }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        public async Task SendProjectDeletedAsync(string userName, string projectName, string projectId)
        {
            var embed = new
            {
                title = "🗑️ Proyecto Eliminado",
                description = $"Usuario eliminó un proyecto",
                color = 15158332, // Rojo
                fields = new[]
                {
                    new { name = "👤 Usuario", value = userName, inline = true },
                    new { name = "📁 Proyecto", value = projectName, inline = true },
                    new { name = "🆔 ID", value = projectId, inline = false }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        // ===== LEADS =====

        public async Task SendLeadCreatedAsync(string projectName, string leadName, string leadEmail, string stage)
        {
            var embed = new
            {
                title = "👥 Nuevo Lead",
                description = $"Se creó un nuevo lead",
                color = 10181046, // Púrpura
                fields = new[]
                {
                    new { name = "📁 Proyecto", value = projectName, inline = true },
                    new { name = "👤 Lead", value = leadName, inline = true },
                    new { name = "📧 Email", value = leadEmail, inline = true },
                    new { name = "📊 Stage", value = stage, inline = true }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        // ===== APPOINTMENTS =====

        public async Task SendAppointmentCreatedAsync(string projectName, string appointmentTitle, string scheduledAt)
        {
            var embed = new
            {
                title = "📅 Nueva Cita",
                description = $"Se agendó una nueva cita",
                color = 3066993, // Verde oscuro
                fields = new[]
                {
                    new { name = "📁 Proyecto", value = projectName, inline = true },
                    new { name = "📋 Título", value = appointmentTitle, inline = true },
                    new { name = "🕒 Fecha", value = scheduledAt, inline = false }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed);
        }

        // ===== ERRORES DEL SERVIDOR =====

        public async Task SendServerErrorAsync(string errorMessage, string stackTrace, string endpoint)
        {
            // Truncar stack trace si es muy largo
            var truncatedStack = stackTrace.Length > 1000 
                ? stackTrace.Substring(0, 1000) + "..." 
                : stackTrace;

            var embed = new
            {
                title = "⚠️ Error del Servidor",
                description = "Se produjo un error en el servidor",
                color = 15158332, // Rojo
                fields = new[]
                {
                    new { name = "🔴 Error", value = errorMessage, inline = false },
                    new { name = "🔗 Endpoint", value = endpoint, inline = true },
                    new { name = "📜 Stack Trace", value = $"```{truncatedStack}```", inline = false }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed, "@here");
        }

        public async Task SendCriticalErrorAsync(string errorMessage, string context)
        {
            var embed = new
            {
                title = "🚨 ERROR CRÍTICO",
                description = "Se produjo un error crítico que requiere atención inmediata",
                color = 10038562, // Rojo oscuro
                fields = new[]
                {
                    new { name = "💥 Error", value = errorMessage, inline = false },
                    new { name = "📍 Contexto", value = context, inline = false }
                },
                timestamp = DateTimeOffset.UtcNow
            };

            await SendEmbedAsync(embed, "@everyone");
        }

        // ===== MÉTODO PRIVADO PARA ENVIAR (Lógica interna) =====

        private async Task SendEmbedAsync(object embed, string content = null!)
        {
            if (!_isEnabled)
            {
                Console.WriteLine("⚠️ Discord webhook está deshabilitado (URL no configurada)");
                return;
            }

            try
            {
                var payload = new
                {
                    content = content,
                    embeds = new[] { embed }
                };

                var json = JsonSerializer.Serialize(payload);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_webhookUrl, httpContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error al enviar webhook a Discord: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción al enviar webhook a Discord: {ex.Message}");
            }
        }
    }
}