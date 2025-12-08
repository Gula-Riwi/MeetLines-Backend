using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Métricas diarias del rendimiento de los bots
    /// </summary>
    public class BotMetrics
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        
        /// <summary>Fecha de la métrica</summary>
        public DateTime Date { get; private set; }
        
        /// <summary>Total de conversaciones</summary>
        public int TotalConversations { get; private set; }
        
        /// <summary>Conversaciones atendidas por bot</summary>
        public int BotConversations { get; private set; }
        
        /// <summary>Conversaciones que requirieron humano</summary>
        public int HumanConversations { get; private set; }
        
        /// <summary>Citas agendadas</summary>
        public int AppointmentsBooked { get; private set; }
        
        /// <summary>Tasa de conversión (%)</summary>
        public double ConversionRate { get; private set; }
        
        /// <summary>Rating promedio de feedback</summary>
        public double? AverageFeedbackRating { get; private set; }
        
        /// <summary>Clientes reactivados</summary>
        public int CustomersReactivated { get; private set; }
        
        /// <summary>Tasa de reactivación (%)</summary>
        public double ReactivationRate { get; private set; }
        
        /// <summary>Tiempo promedio de respuesta (segundos)</summary>
        public double AverageResponseTime { get; private set; }
        
        /// <summary>Satisfacción del cliente (0-100)</summary>
        public double CustomerSatisfactionScore { get; private set; }
        
        public DateTime CreatedAt { get; private set; }

        // EF Core constructor
        private BotMetrics() { }

        public BotMetrics(
            Guid projectId,
            DateTime date,
            int totalConversations,
            int botConversations,
            int humanConversations,
            int appointmentsBooked,
            double conversionRate,
            int customersReactivated,
            double reactivationRate,
            double averageResponseTime,
            double customerSatisfactionScore,
            double? averageFeedbackRating = null)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (totalConversations < 0) throw new ArgumentException("TotalConversations cannot be negative", nameof(totalConversations));
            if (botConversations < 0) throw new ArgumentException("BotConversations cannot be negative", nameof(botConversations));
            if (humanConversations < 0) throw new ArgumentException("HumanConversations cannot be negative", nameof(humanConversations));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            Date = date.Date; // Ensure only date part
            TotalConversations = totalConversations;
            BotConversations = botConversations;
            HumanConversations = humanConversations;
            AppointmentsBooked = appointmentsBooked;
            ConversionRate = conversionRate;
            AverageFeedbackRating = averageFeedbackRating;
            CustomersReactivated = customersReactivated;
            ReactivationRate = reactivationRate;
            AverageResponseTime = averageResponseTime;
            CustomerSatisfactionScore = customerSatisfactionScore;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateMetrics(
            int totalConversations,
            int botConversations,
            int humanConversations,
            int appointmentsBooked,
            double conversionRate,
            int customersReactivated,
            double reactivationRate,
            double averageResponseTime,
            double customerSatisfactionScore,
            double? averageFeedbackRating = null)
        {
            TotalConversations = totalConversations;
            BotConversations = botConversations;
            HumanConversations = humanConversations;
            AppointmentsBooked = appointmentsBooked;
            ConversionRate = conversionRate;
            AverageFeedbackRating = averageFeedbackRating;
            CustomersReactivated = customersReactivated;
            ReactivationRate = reactivationRate;
            AverageResponseTime = averageResponseTime;
            CustomerSatisfactionScore = customerSatisfactionScore;
        }
    }
}
