using System;
using System.Collections.Generic;

namespace MeetLines.Application.DTOs.Config
{
    public class ReactivationConfig
    {
        public bool Enabled { get; set; }
        public List<string> Messages { get; set; } = new List<string>(); // Lista de mensajes por intento
        public int DelayDays { get; set; } = 30; // Días de inactividad para activar (antes DaysThreshold)
        public int MaxAttempts { get; set; } = 3; // Máximo de intentos
        public int DaysBetweenAttempts { get; set; } = 30; // Días entre mensajes (antes CoolDownDays)
        public string? CustomPrompt { get; set; }
        
        public bool OfferDiscount { get; set; }
        public string? DiscountMessage { get; set; } // Mensaje específico de descuento
        public int DiscountPercentage { get; set; }
    }
}
