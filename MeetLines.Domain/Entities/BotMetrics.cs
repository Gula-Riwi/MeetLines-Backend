using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Métricas diarias del rendimiento de los bots
    /// </summary>
    public class BotMetrics
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        
        /// <summary>Fecha de la métrica</summary>
        public DateTime Date { get; set; }
        
        /// <summary>Total de conversaciones</summary>
        public int TotalConversations { get; set; }
        
        /// <summary>Conversaciones atendidas por bot</summary>
        public int BotConversations { get; set; }
        
        /// <summary>Conversaciones que requirieron humano</summary>
        public int HumanConversations { get; set; }
        
        /// <summary>Citas agendadas</summary>
        public int AppointmentsBooked { get; set; }
        
        /// <summary>Tasa de conversión (%)</summary>
        public double ConversionRate { get; set; }
        
        /// <summary>Rating promedio de feedback</summary>
        public double? AverageFeedbackRating { get; set; }
        
        /// <summary>Clientes reactivados</summary>
        public int CustomersReactivated { get; set; }
        
        /// <summary>Tasa de reactivación (%)</summary>
        public double ReactivationRate { get; set; }
        
        /// <summary>Tiempo promedio de respuesta (segundos)</summary>
        public double AverageResponseTime { get; set; }
        
        /// <summary>Satisfacción del cliente (0-100)</summary>
        public double CustomerSatisfactionScore { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation
        public Project? Project { get; set; }
    }
}
