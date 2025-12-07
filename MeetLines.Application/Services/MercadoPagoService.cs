using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

// --- MERCADOPAGO SDK ---
using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Preference;
// [IMPORTANTE] ALIAS 1: Usamos 'MPPayment' para referirnos a la respuesta de la API
using MPPayment = MercadoPago.Resource.Payment.Payment; 

// --- DOMINIO ---
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Payment;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;
// [IMPORTANTE] ALIAS 2: Usamos 'DomainPayment' para referirnos a tu Base de Datos
using DomainPayment = MeetLines.Domain.Entities.Payment;
using MeetLines.Domain.Entities; 

namespace MeetLines.Infrastructure.Services
{
    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ISaasUserRepository _userRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IDiscordWebhookService _discordWebhookService;
        private readonly IConfiguration _configuration;
        private readonly string _accessToken;
        private readonly string _frontendUrl;

        // Definición de planes (Precios temporales en 0)
        private readonly Dictionary<string, (decimal Price, string Description)> _plans = new()
        {
            { "beginner",     (0m, "Plan Beginner - Descripción pendiente") },
            { "intermediate", (0m, "Plan Intermediate - Descripción pendiente") },
            { "complete",     (0m, "Plan Complete - Descripción pendiente") }
        };

        public MercadoPagoService(
            IPaymentRepository paymentRepository,
            ISaasUserRepository userRepository,
            ISubscriptionRepository subscriptionRepository,
            IDiscordWebhookService discordWebhookService,
            IConfiguration configuration)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
            _discordWebhookService = discordWebhookService ?? throw new ArgumentNullException(nameof(discordWebhookService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            _accessToken = _configuration["MercadoPago:AccessToken"] 
                ?? throw new ArgumentException("MercadoPago:AccessToken is missing");
            _frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:3000";

            // Configurar SDK de Mercado Pago
            MercadoPagoConfig.AccessToken = _accessToken;
        }

        public async Task<Result<CreatePaymentPreferenceResponse>> CreatePaymentPreferenceAsync(
            Guid userId, 
            CreatePaymentPreferenceRequest request, 
            CancellationToken ct = default)
        {
            try
            {
                // Normalizamos el input a minúsculas para evitar errores (Beginner vs beginner)
                var planKey = request.Plan.ToLower().Trim();

                if (!_plans.ContainsKey(planKey))
                    return Result<CreatePaymentPreferenceResponse>.Fail($"Plan '{request.Plan}' no válido");

                var user = await _userRepository.GetByIdAsync(userId, ct);
                if (user == null)
                    return Result<CreatePaymentPreferenceResponse>.Fail("Usuario no encontrado");

                var (price, description) = _plans[planKey];

                if (price <= 0)
                    return Result<CreatePaymentPreferenceResponse>.Fail("El precio del plan no está configurado (es 0). Contacte a soporte.");

                // 1. Usamos el alias DomainPayment para crear el pago en DB
                var payment = new DomainPayment(userId, planKey, price);
                await _paymentRepository.AddAsync(payment, ct);

                // 2. Crear preferencia en Mercado Pago
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
                        Success = $"{_frontendUrl}/payment/success",
                        Failure = $"{_frontendUrl}/payment/failure",
                        Pending = $"{_frontendUrl}/payment/pending"
                    },
                    AutoReturn = "approved",
                    ExternalReference = payment.Id.ToString(), 
                    NotificationUrl = $"{_configuration["Api:BaseUrl"]}/api/Payment/webhook",
                    StatementDescriptor = "MeetLines"
                };

                var client = new PreferenceClient();
                Preference preference = await client.CreateAsync(preferenceRequest, cancellationToken: ct);

                var response = new CreatePaymentPreferenceResponse
                {
                    PaymentId = payment.Id,
                    PreferenceId = preference.Id,
                    InitPoint = preference.InitPoint,
                    SandboxInitPoint = preference.SandboxInitPoint,
                    Plan = planKey,
                    Amount = price
                };

                return Result<CreatePaymentPreferenceResponse>.Ok(response);
            }
            catch (Exception ex)
            {
                return Result<CreatePaymentPreferenceResponse>.Fail($"Error al crear preferencia: {ex.Message}");
            }
        }

        public async Task<Result> ProcessWebhookNotificationAsync(long paymentId, CancellationToken ct = default)
        {
            try
            {
                var client = new PaymentClient();
                
                // Usamos MPPayment (el alias)
                MPPayment mpPayment = await client.GetAsync(paymentId, cancellationToken: ct);

                if (mpPayment == null)
                    return Result.Fail("Pago no encontrado en Mercado Pago");

                var externalRef = mpPayment.ExternalReference;
                if (string.IsNullOrEmpty(externalRef) || !Guid.TryParse(externalRef, out var paymentGuid))
                    return Result.Fail("External reference inválido");

                // Usamos el alias implícito DomainPayment
                var payment = await _paymentRepository.GetByIdAsync(paymentGuid, ct);
                if (payment == null)
                    return Result.Fail("Pago no encontrado en base de datos");

                var user = await _userRepository.GetByIdAsync(payment.UserId, ct);
                if (user == null)
                    return Result.Fail("Usuario no encontrado");

                string status = mpPayment.Status ?? "unknown";
                string statusDetail = mpPayment.StatusDetail ?? "no_detail";
                long mpId = mpPayment.Id ?? 0;

                switch (status)
                {
                    case "approved":
                        var currentSubscription = await _subscriptionRepository.GetActiveByUserIdAsync(payment.UserId, ct);
                        
                        if (currentSubscription != null && currentSubscription.Plan != payment.Plan)
                        {
                            currentSubscription.Cancel();
                            await _subscriptionRepository.UpdateAsync(currentSubscription, ct);
                            
                            await _discordWebhookService.SendSubscriptionUpgradedAsync(
                                user.Name, user.Email, currentSubscription.Plan, payment.Plan);
                        }
                        
                        var newSubscription = new Subscription(
                            userId: payment.UserId,
                            plan: payment.Plan,
                            cycle: "monthly",
                            price: payment.Amount
                        );
                        await _subscriptionRepository.AddAsync(newSubscription, ct);

                        payment.MarkAsApproved(mpId, status, statusDetail, newSubscription.Id);
                        await _paymentRepository.UpdateAsync(payment, ct);

                        if (currentSubscription == null || currentSubscription.Plan == "Gratuito")
                        {
                            await _discordWebhookService.SendSubscriptionCreatedAsync(
                                user.Name, user.Email, payment.Plan, payment.Amount);
                        }
                        break;

                    case "rejected":
                    case "cancelled":
                        payment.MarkAsRejected(status, statusDetail, statusDetail);
                        await _paymentRepository.UpdateAsync(payment, ct);
                        break;

                    case "pending":
                    case "in_process":
                        payment.MarkAsPending(status, statusDetail);
                        await _paymentRepository.UpdateAsync(payment, ct);
                        break;
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _discordWebhookService.SendServerErrorAsync(
                    $"Error procesando webhook de Mercado Pago: {ex.Message}",
                    ex.StackTrace ?? "",
                    "ProcessWebhookNotificationAsync"
                );
                return Result.Fail($"Error al procesar webhook: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<PaymentHistoryResponse>>> GetPaymentHistoryAsync(
            Guid userId, 
            CancellationToken ct = default)
        {
            try
            {
                var payments = await _paymentRepository.GetByUserIdAsync(userId, ct);
                var safePayments = payments ?? Enumerable.Empty<DomainPayment>();

                var response = safePayments.Select(p => new PaymentHistoryResponse
                {
                    Id = p.Id,
                    Plan = p.Plan,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status,
                    MercadoPagoStatus = p.MercadoPagoStatus,
                    MercadoPagoStatusDetail = p.MercadoPagoStatusDetail,
                    ErrorMessage = p.ErrorMessage,
                    CreatedAt = p.CreatedAt,
                    ProcessedAt = p.ProcessedAt
                });

                return Result<IEnumerable<PaymentHistoryResponse>>.Ok(response);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<PaymentHistoryResponse>>.Fail($"Error al obtener historial: {ex.Message}");
            }
        }
    }
}