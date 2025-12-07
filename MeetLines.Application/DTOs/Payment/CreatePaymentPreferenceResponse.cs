using System;

namespace MeetLines.Application.DTOs.Payment
{
    public class CreatePaymentPreferenceResponse
    {
        public Guid PaymentId { get; set; }
        public string PreferenceId { get; set; } = string.Empty;
        public string InitPoint { get; set; } = string.Empty; // URL para redirigir al usuario
        public string SandboxInitPoint { get; set; } = string.Empty; // URL de test
        public string Plan { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}