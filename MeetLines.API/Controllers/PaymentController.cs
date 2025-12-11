using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.Payment;
using MeetLines.Application.Services.Interfaces;
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

        /// <summary>
        /// Crea un pago en Mercado Pago
        /// POST: api/payment/create
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CreatePaymentResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            if (userId == Guid.Empty)
                return Unauthorized(ApiResponse.Fail("Token inválido"));

            var result = await _mercadoPagoService.CreatePaymentAsync(userId, request, ct);

            if (!result.Success)
                return BadRequest(ApiResponse<CreatePaymentResponse>.Fail(result.ErrorMessage ?? "Error al crear pago"));

            return Ok(ApiResponse<CreatePaymentResponse>.Ok(result.Data!, "Redirige al usuario a la URL de pago"));
        }

        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}