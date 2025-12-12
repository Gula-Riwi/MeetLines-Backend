using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Payment;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ISaasUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IDiscordWebhookService _discordService;
        private readonly string _frontendUrl;

        // PLANES CON PRECIO $0 PARA PRUEBAS
        private readonly Dictionary<string, (decimal Price, string Description)> _plans = new()
        {
            { "beginner",     (0m, "Plan Beginner - Prueba") },
            { "intermediate", (0m, "Plan Intermediate - Prueba") },
            { "complete",     (0m, "Plan Complete - Prueba") }
        };

        public MercadoPagoService(
            IPaymentRepository paymentRepository,
            ISaasUserRepository userRepository,
            IConfiguration configuration,
            IDiscordWebhookService discordService)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _discordService = discordService ?? throw new ArgumentNullException(nameof(discordService));
            
            var accessToken = _configuration["MercadoPago:AccessToken"];
            
            // Resolver placeholder ${VAR} si existe, ya que IConfiguration no lo hace automático para estos valores
            if (!string.IsNullOrEmpty(accessToken) && accessToken.StartsWith("${") && accessToken.EndsWith("}"))
            {
                var envVar = accessToken.Trim('$', '{', '}');
                accessToken = Environment.GetEnvironmentVariable(envVar) ?? accessToken;
            }

            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("MercadoPago:AccessToken is missing in configuration");
            
            _frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:3000";

            // Configurar SDK de Mercado Pago
            MercadoPagoConfig.AccessToken = accessToken;
        }

        public async Task<Result<CreatePaymentResponse>> CreatePaymentAsync(
            Guid userId, 
            CreatePaymentRequest request, 
            CancellationToken ct = default)
        {
            try
            {
                // Normalizar plan
                var planKey = request.Plan.ToLower().Trim();

                if (!_plans.ContainsKey(planKey))
                    return Result<CreatePaymentResponse>.Fail($"Plan '{request.Plan}' no válido. Planes disponibles: beginner, intermediate, complete");

                // Obtener usuario
                var user = await _userRepository.GetByIdAsync(userId, ct);
                if (user == null)
                    return Result<CreatePaymentResponse>.Fail("Usuario no encontrado");

                var (price, description) = _plans[planKey];

                // Crear registro de pago en BD
                var payment = new Payment(userId, planKey, price);
                await _paymentRepository.AddAsync(payment, ct);

                var successUrl = $"{_frontendUrl}/payment/success?payment_id={payment.Id}";
                var failureUrl = $"{_frontendUrl}/payment/failure?payment_id={payment.Id}";
                var pendingUrl = $"{_frontendUrl}/payment/pending?payment_id={payment.Id}";

                // Crear preferencia en Mercado Pago
                var preferenceRequest = new PreferenceRequest
                {
                    Items = new List<PreferenceItemRequest>
                    {
                        new PreferenceItemRequest
                        {
                            Title = description,
                            Quantity = 1,
                            CurrencyId = "COP",
                            UnitPrice = price,
                        }
                    },
                    Payer = new PreferencePayerRequest
                    {
                        Email = user.Email,
                        Name = user.Name
                    },
                    BackUrls = new PreferenceBackUrlsRequest
                    {
                        Success = successUrl,
                        Failure = failureUrl,
                        Pending = pendingUrl
                    },
                    AutoReturn = "approved",
                    ExternalReference = payment.Id.ToString(),
                    StatementDescriptor = "MeetLines"
                };

                var client = new PreferenceClient();
                Preference preference = await client.CreateAsync(preferenceRequest, cancellationToken: ct);

                var response = new CreatePaymentResponse
                {
                    PaymentId = payment.Id,
                    PreferenceId = preference.Id,
                    CheckoutUrl = preference.InitPoint, // URL para redirigir al usuario
                    Plan = planKey,
                    Amount = price
                };

                return Result<CreatePaymentResponse>.Ok(response);
            }
            catch (MercadoPago.Error.MercadoPagoApiException mpEx)
            {
                // Loguear error específico de Mercado Pago (usar ILogger en producción)
                Console.WriteLine($"[Error] Mercado Pago API: {mpEx.Message} | StatusCode: {mpEx.StatusCode}");
                return Result<CreatePaymentResponse>.Fail($"Error Mercado Pago: {mpEx.ApiError?.Message ?? mpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] CreatePayment: {ex.Message}");
                return Result<CreatePaymentResponse>.Fail($"Error al crear pago: {ex.Message}");
            }
        }
    }
}