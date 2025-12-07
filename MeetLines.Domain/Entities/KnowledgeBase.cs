using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Base de conocimiento (FAQ) por proyecto
    /// </summary>
    public class KnowledgeBase
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        
        /// <summary>Categoría de la pregunta</summary>
        public string Category { get; set; } = "general"; // services, pricing, hours, policies, location, faq
        
        /// <summary>Pregunta frecuente</summary>
        public string Question { get; set; } = string.Empty;
        
        /// <summary>Respuesta</summary>
        public string Answer { get; set; } = string.Empty;
        
        /// <summary>Palabras clave para matching (JSON array)</summary>
        public string Keywords { get; set; } = "[]";
        
        /// <summary>Prioridad (mayor = más importante)</summary>
        public int Priority { get; set; } = 0;
        
        /// <summary>Activa/Inactiva</summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>Contador de veces que se ha usado</summary>
        public int UsageCount { get; set; } = 0;
        
        /// <summary>Última vez que se usó</summary>
        public DateTime? LastUsedAt { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation
        public Project? Project { get; set; }
    }
}
