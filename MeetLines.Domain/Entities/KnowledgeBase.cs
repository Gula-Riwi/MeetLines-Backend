using System;

namespace MeetLines.Domain.Entities
{
    /// <summary>
    /// Base de conocimiento (FAQ) por proyecto
    /// </summary>
    public class KnowledgeBase
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        
        /// <summary>Categoría de la pregunta</summary>
        public string Category { get; private set; }
        
        /// <summary>Pregunta frecuente</summary>
        public string Question { get; private set; }
        
        /// <summary>Respuesta</summary>
        public string Answer { get; private set; }
        
        /// <summary>Palabras clave para matching (JSON array)</summary>
        public string Keywords { get; private set; }
        
        /// <summary>Prioridad (mayor = más importante)</summary>
        public int Priority { get; private set; }
        
        /// <summary>Activa/Inactiva</summary>
        public bool IsActive { get; private set; }
        
        /// <summary>Contador de veces que se ha usado</summary>
        public int UsageCount { get; private set; }
        
        /// <summary>Última vez que se usó</summary>
        public DateTime? LastUsedAt { get; private set; }
        
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // EF Core constructor
        private KnowledgeBase() 
        { 
            Category = null!;
            Question = null!;
            Answer = null!;
            Keywords = null!;
        }

        public KnowledgeBase(Guid projectId, string category, string question, string answer, string? keywords = null, int priority = 0)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("ProjectId cannot be empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(question)) throw new ArgumentException("Question cannot be empty", nameof(question));
            if (string.IsNullOrWhiteSpace(answer)) throw new ArgumentException("Answer cannot be empty", nameof(answer));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            Category = category ?? "general";
            Question = question;
            Answer = answer;
            Keywords = keywords ?? "[]";
            Priority = priority;
            IsActive = true;
            UsageCount = 0;
            LastUsedAt = null;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string? category, string? question, string? answer, string? keywords, int? priority)
        {
            if (!string.IsNullOrWhiteSpace(category))
                Category = category;
            
            if (!string.IsNullOrWhiteSpace(question))
                Question = question;
            
            if (!string.IsNullOrWhiteSpace(answer))
                Answer = answer;
            
            if (keywords != null)
                Keywords = keywords;
            
            if (priority.HasValue)
                Priority = priority.Value;

            UpdatedAt = DateTime.UtcNow;
        }

        public void RecordUsage()
        {
            UsageCount++;
            LastUsedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
