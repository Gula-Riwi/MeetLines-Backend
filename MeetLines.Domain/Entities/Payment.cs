using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Entidad para registrar pagos de Mercado Pago
    /// </summary>
    public class Payment
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid? SubscriptionId { get; private set; }
        
        // Datos de Mercado Pago
        public long? MercadoPagoPaymentId { get; private set; }
        public string? MercadoPagoPreferenceId { get; private set; }
        public string? MercadoPagoStatus { get; private set; }
        public string? MercadoPagoStatusDetail { get; private set; }
        
        // Datos del pago
        public string Plan { get; private set; } = null!;
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "COP"; // Peso Colombiano
        public string PaymentMethod { get; private set; } = null!;
        
        // Estados
        public string Status { get; private set; } = "pending"; // pending, approved, rejected, cancelled
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? ProcessedAt { get; private set; }
        
        // Metadata
        public string? ErrorMessage { get; private set; }

        // Constructor privado para EF Core
        private Payment() { }

        public Payment(
            Guid userId, 
            string plan, 
            decimal amount, 
            string? mercadoPagoPreferenceId = null)
        {
            if (userId == Guid.Empty) 
                throw new ArgumentException("UserId cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(plan)) 
                throw new ArgumentException("Plan cannot be empty", nameof(plan));
            if (amount < 0) 
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            Id = Guid.NewGuid();
            UserId = userId;
            Plan = plan;
            Amount = amount;
            MercadoPagoPreferenceId = mercadoPagoPreferenceId;
            Status = "pending";
            PaymentMethod = "mercadopago";
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsApproved(long mercadoPagoPaymentId, string status, string statusDetail, Guid? subscriptionId = null)
        {
            MercadoPagoPaymentId = mercadoPagoPaymentId;
            MercadoPagoStatus = status;
            MercadoPagoStatusDetail = statusDetail;
            SubscriptionId = subscriptionId;
            Status = "approved";
            ProcessedAt = DateTimeOffset.UtcNow;
        }

        // CORRECCIÓN AQUÍ: Se agregó el '?' en 'string? errorMessage'
        public void MarkAsRejected(string status, string statusDetail, string? errorMessage = null)
        {
            MercadoPagoStatus = status;
            MercadoPagoStatusDetail = statusDetail;
            Status = "rejected";
            ErrorMessage = errorMessage;
            ProcessedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsPending(string status, string statusDetail)
        {
            MercadoPagoStatus = status;
            MercadoPagoStatusDetail = statusDetail;
            Status = "pending";
        }

        public void Cancel()
        {
            Status = "cancelled";
            ProcessedAt = DateTimeOffset.UtcNow;
        }
    }
}