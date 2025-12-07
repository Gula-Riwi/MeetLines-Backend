using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Application.DTOs.Payment;
using MeetLines.API.DTOs;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IMercadoPagoService _mercadoPagoService;

        public PaymentController(IMercadoPagoService mercadoPagoService)
        {
            _mercadoPagoService = mercadoPagoService ?? throw new ArgumentNullException(nameof(mercadoPagoService));
        }

        [Authorize]
        [HttpPost("create-preference")]
        public async Task<IActionResult> CreatePreference([FromBody] CreatePaymentPreferenceRequest request, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse.Fail("Usuario no autenticado correctamente."));
            }

            var result = await _mercadoPagoService.CreatePaymentPreferenceAsync(userId, request, ct);

            if (!result.IsSuccess)
            {
                // CORRECCIÓN 1: Usamos '??' para asegurar que el mensaje de error no sea nulo
                return BadRequest(ApiResponse.Fail(result.Error ?? "Error desconocido al crear preferencia."));
            }

            // CORRECCIÓN 2: Usamos '!' para asegurar que Value no es nulo (porque ya validamos IsSuccess)
            return Ok(ApiResponse<CreatePaymentPreferenceResponse>.Ok(result.Value!));
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromQuery] string topic, [FromQuery] long? id, CancellationToken ct)
        {
            // MercadoPago envía ?topic=payment&id=123456
            if (id.HasValue && topic == "payment")
            {
                var result = await _mercadoPagoService.ProcessWebhookNotificationAsync(id.Value, ct);
                
                if (!result.IsSuccess)
                {
                    // CORRECCIÓN 3: Usamos '??' para el error
                    return BadRequest(ApiResponse.Fail(result.Error ?? "Error procesando el webhook."));
                }
            }
            
            // Siempre devolver 200 OK a MercadoPago para confirmar recepción
            return Ok();
        }

        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse.Fail("Usuario no autenticado correctamente."));
            }

            var result = await _mercadoPagoService.GetPaymentHistoryAsync(userId, ct);

            if (!result.IsSuccess)
            {
                // CORRECCIÓN 4: Usamos '??' para el error
                return BadRequest(ApiResponse.Fail(result.Error ?? "Error obteniendo el historial."));
            }

            // CORRECCIÓN 5: Usamos '!' para asegurar que Value no es nulo
            return Ok(ApiResponse<IEnumerable<PaymentHistoryResponse>>.Ok(result.Value!));
        }
    }
}