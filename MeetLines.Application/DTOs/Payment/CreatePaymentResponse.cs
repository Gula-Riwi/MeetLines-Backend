using System;

namespace MeetLines.Application.DTOs.Payment
{
    public class CreatePaymentResponse
    {
        public Guid PaymentId { get; set; }
        public string PreferenceId { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}