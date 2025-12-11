using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Payment;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IMercadoPagoService
    {
        Task<Result<CreatePaymentResponse>> CreatePaymentAsync(Guid userId, CreatePaymentRequest request, CancellationToken ct = default);
    }
}