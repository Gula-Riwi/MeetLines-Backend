using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Payment;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IMercadoPagoService
    {
        Task<Result<CreatePaymentPreferenceResponse>> CreatePaymentPreferenceAsync(
            Guid userId, 
            CreatePaymentPreferenceRequest request, 
            CancellationToken ct = default);
        
        Task<Result> ProcessWebhookNotificationAsync(
            long paymentId, 
            CancellationToken ct = default);
        
        Task<Result<IEnumerable<PaymentHistoryResponse>>> GetPaymentHistoryAsync(
            Guid userId, 
            CancellationToken ct = default);
    }
}