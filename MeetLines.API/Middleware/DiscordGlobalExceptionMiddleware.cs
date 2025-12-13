using System.Net;
using System.Text.Json;
using MeetLines.Application.Services.Interfaces;

namespace MeetLines.API.Middlewares
{
    public class DiscordGlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public DiscordGlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IDiscordWebhookService discordService)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // 1. Enviar alerta a Discord
                await HandleExceptionToDiscordAsync(context, ex, discordService);

                // 2. Responder al cliente (Frontend)
                await HandleExceptionResponseAsync(context, ex);
            }
        }

        private async Task HandleExceptionToDiscordAsync(HttpContext context, Exception ex, IDiscordWebhookService discordService)
        {
            try
            {
                var request = context.Request;
                // Usamos el método específico para errores de servidor que ya tienes en tu servicio
                await discordService.SendServerErrorAsync(
                    errorMessage: ex.Message,
                    stackTrace: ex.StackTrace ?? "No stack trace available",
                    endpoint: $"{request.Method} {request.Path}"
                );
            }
            catch (Exception discordEx)
            {
                // Si falla el envío a Discord, solo lo escribimos en consola para no perder el error original
                Console.WriteLine($"Error al enviar log a Discord: {discordEx.Message}");
            }
        }

        private static Task HandleExceptionResponseAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                error = "Ocurrió un error interno en el servidor.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}