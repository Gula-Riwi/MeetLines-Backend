using System;

namespace MeetLines.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        
        // Datos de Mercado Pago
        public string? MercadoPagoPreferenceId { get; private set; }
        
        // Datos del pago
        public string Plan { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public string Status { get; private set; }
        
        public DateTimeOffset CreatedAt { get; private set; }

        // Constructor privado para EF Core
        private Payment() 
        {
            Plan = null!;
            Currency = null!;
            Status = null!;
        }

        public Payment(Guid userId, string plan, decimal amount, string? preferenceId = null)
        {
            if (userId == Guid.Empty) 
                throw new ArgumentException("UserId cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(plan)) 
                throw new ArgumentException("Plan cannot be empty", nameof(plan));

            Id = Guid.NewGuid();
            UserId = userId;
            Plan = plan;
            Amount = amount;
            Currency = "COP";
            Status = "pending";
            MercadoPagoPreferenceId = preferenceId;
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}