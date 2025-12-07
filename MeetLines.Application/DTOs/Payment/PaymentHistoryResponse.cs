using System;

namespace MeetLines.Application.DTOs.Payment
{
    public class PaymentHistoryResponse
    {
        public Guid Id { get; set; }
        public string Plan { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // pending, approved, rejected, cancelled
        public string? MercadoPagoStatus { get; set; }
        public string? MercadoPagoStatusDetail { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
    }
}