namespace MeetLines.Application.DTOs.Payment
{
    public class CreatePaymentPreferenceRequest
    {
        public string Plan { get; set; } = string.Empty; // Basic, Pro, Enterprise
    }
}