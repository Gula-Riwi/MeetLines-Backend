using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.DTOs.BotSystem;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Domain.ValueObjects;

namespace MeetLines.Application.Services
{
    /// <summary>
    /// Service for managing bot configurations
    /// </summary>
    public class ProjectBotConfigService : IProjectBotConfigService
    {
        private readonly IProjectBotConfigRepository _repository;
        private readonly IProjectRepository _projectRepository;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProjectBotConfigService(IProjectBotConfigRepository repository, IProjectRepository projectRepository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<ProjectBotConfigDto?> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            var config = await _repository.GetByProjectIdAsync(projectId, ct);
            return config == null ? null : MapToDto(config);
        }

        public async Task<ProjectBotConfigDto> CreateAsync(CreateProjectBotConfigRequest request, Guid createdBy, CancellationToken ct = default)
        {
            // Validate that the project belongs to the user
            var isOwner = await _projectRepository.IsUserProjectOwnerAsync(createdBy, request.ProjectId, ct);
            if (!isOwner)
            {
                throw new UnauthorizedAccessException($"User {createdBy} does not have permission to create bot configuration for project {request.ProjectId}");
            }

            // Get industry defaults
            var defaults = GetIndustryDefaultsInternal(request.Industry);

            var config = new ProjectBotConfig
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                BotName = request.BotName ?? defaults.BotName,
                Industry = request.Industry,
                Tone = request.Tone ?? defaults.Tone,
                Timezone = request.Timezone ?? defaults.Timezone,
                ReceptionConfigJson = JsonSerializer.Serialize(defaults.ReceptionConfig, _jsonOptions),
                TransactionalConfigJson = JsonSerializer.Serialize(defaults.TransactionalConfig, _jsonOptions),
                FeedbackConfigJson = JsonSerializer.Serialize(defaults.FeedbackConfig, _jsonOptions),
                ReactivationConfigJson = JsonSerializer.Serialize(defaults.ReactivationConfig, _jsonOptions),
                IntegrationsConfigJson = JsonSerializer.Serialize(defaults.IntegrationsConfig, _jsonOptions),
                AdvancedConfigJson = JsonSerializer.Serialize(defaults.AdvancedConfig, _jsonOptions),
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _repository.CreateAsync(config, ct);
            return MapToDto(created);
        }

        public async Task<ProjectBotConfigDto> UpdateAsync(Guid projectId, UpdateProjectBotConfigRequest request, Guid updatedBy, CancellationToken ct = default)
        {
            var config = await _repository.GetByProjectIdAsync(projectId, ct);
            if (config == null)
            {
                throw new InvalidOperationException($"Bot configuration not found for project {projectId}");
            }

            // Update only provided fields
            if (request.BotName != null) config.BotName = request.BotName;
            if (request.Tone != null) config.Tone = request.Tone;
            
            if (request.ReceptionConfig != null)
                config.ReceptionConfigJson = JsonSerializer.Serialize(request.ReceptionConfig, _jsonOptions);
            
            if (request.TransactionalConfig != null)
                config.TransactionalConfigJson = JsonSerializer.Serialize(request.TransactionalConfig, _jsonOptions);
            
            if (request.FeedbackConfig != null)
                config.FeedbackConfigJson = JsonSerializer.Serialize(request.FeedbackConfig, _jsonOptions);
            
            if (request.ReactivationConfig != null)
                config.ReactivationConfigJson = JsonSerializer.Serialize(request.ReactivationConfig, _jsonOptions);
            
            if (request.IntegrationsConfig != null)
                config.IntegrationsConfigJson = JsonSerializer.Serialize(request.IntegrationsConfig, _jsonOptions);
            
            if (request.AdvancedConfig != null)
                config.AdvancedConfigJson = JsonSerializer.Serialize(request.AdvancedConfig, _jsonOptions);

            config.UpdatedBy = updatedBy;
            config.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(config, ct);
            return MapToDto(config);
        }

        public async Task DeleteAsync(Guid projectId, CancellationToken ct = default)
        {
            var config = await _repository.GetByProjectIdAsync(projectId, ct);
            if (config != null)
            {
                await _repository.DeleteAsync(config.Id, ct);
            }
        }

        public ProjectBotConfigDto GetIndustryDefaults(string industry)
        {
            return GetIndustryDefaultsInternal(industry);
        }

        private ProjectBotConfigDto GetIndustryDefaultsInternal(string industry)
        {
            var defaults = new ProjectBotConfigDto
            {
                BotName = "Asistente Virtual",
                Industry = industry,
                Tone = "friendly",
                Timezone = "America/Bogota",
                ReceptionConfig = new ReceptionBotConfig
                {
                    Enabled = true,
                    WelcomeMessage = GetWelcomeMessage(industry),
                    IntentTriggerKeywords = GetIntentKeywords(industry),
                    HandoffMessage = "¡Perfecto! Te ayudo con eso enseguida 📅",
                    OutOfHoursMessage = "Gracias por contactarnos. Nuestro horario es {hours}. Te responderemos pronto."
                },
                TransactionalConfig = new TransactionalBotConfig
                {
                    Enabled = true,
                    AppointmentDurationMinutes = GetDefaultDuration(industry),
                    BufferMinutes = 0,
                    MaxAdvanceBookingDays = 30,
                    MinAdvanceBookingDays = 0,
                    ConfirmationMessage = "✅ ¡Listo! Tu cita está confirmada para el {date} a las {time}.",
                    SendReminder = true,
                    ReminderHoursBefore = 24,
                    ReminderMessage = "Hola {customerName}, te recordamos tu cita mañana a las {time}.",
                    AllowCancellation = true,
                    MinCancellationHours = 24
                },
                FeedbackConfig = new FeedbackBotConfig
                {
                    Enabled = true,
                    DelayHours = 24,
                    RequestMessage = "Hola {customerName}, ¿cómo calificarías tu experiencia del 1 al 5?",
                    NegativeFeedbackMessage = "Lamentamos eso. ¿Qué podemos mejorar?",
                    NotifyOwnerOnNegative = true
                },
                ReactivationConfig = new ReactivationBotConfig
                {
                    Enabled = true,
                    DelayDays = GetReactivationDelay(industry),
                    MaxAttempts = 3,
                    DaysBetweenAttempts = 30,
                    Messages = new System.Collections.Generic.List<string>
                    {
                        "Hola {customerName}, hace {days} días no te vemos. ¿Te gustaría agendar?",
                        "Hola {customerName}, ¿cómo has estado? Tenemos disponibilidad esta semana.",
                        "Hola {customerName}, te extrañamos. ¿Podemos ayudarte en algo?"
                    },
                    OfferDiscount = false,
                    DiscountPercentage = 10,
                    DiscountMessage = "¡Tenemos un {discount}% de descuento para ti!"
                },
                IntegrationsConfig = new IntegrationsConfig
                {
                    Payments = new PaymentIntegration
                    {
                        Enabled = false,
                        RequireAdvancePayment = false,
                        AdvancePaymentPercentage = 50
                    }
                },
                AdvancedConfig = new AdvancedBotConfig
                {
                    HumanFallback = true,
                    HumanFallbackKeywords = "hablar con persona,hablar con humano,operador",
                    HumanFallbackMessage = "Te conecto con un miembro de nuestro equipo.",
                    MultiAgent = false,
                    AgentAssignmentStrategy = "round-robin",
                    TestMode = false
                }
            };

            return defaults;
        }

        private string GetWelcomeMessage(string industry)
        {
            return industry.ToLower() switch
            {
                "barbershop" => "¡Hola! Soy {botName}, el asistente de {businessName}. ¿Quieres agendar un corte? 💈",
                "lawyer" => "Hola, soy {botName} de {businessName}. ¿En qué podemos asesorarte? ⚖️",
                "spa" => "¡Hola! Soy {botName} de {businessName}. ¿Te gustaría reservar un tratamiento? 🧖",
                "clinic" => "Hola, soy {botName} de {businessName}. ¿Necesitas agendar una consulta? 🏥",
                "gym" => "¡Hola! Soy {botName} de {businessName}. ¿Quieres información sobre nuestras clases? 💪",
                _ => "¡Hola! Soy {botName}, el asistente virtual de {businessName}. ¿En qué puedo ayudarte?"
            };
        }

        private string GetIntentKeywords(string industry)
        {
            return industry.ToLower() switch
            {
                "barbershop" => "agendar,reservar,cita,corte,barba,afeitado",
                "lawyer" => "consulta,asesoría,cita,abogado,legal",
                "spa" => "reservar,masaje,facial,tratamiento,spa",
                "clinic" => "cita,consulta,médico,doctor,examen",
                "gym" => "clase,entrenamiento,membresía,inscripción",
                _ => "agendar,reservar,cita,comprar,información"
            };
        }

        private int GetDefaultDuration(string industry)
        {
            return industry.ToLower() switch
            {
                "barbershop" => 45,
                "lawyer" => 60,
                "spa" => 90,
                "clinic" => 30,
                "gym" => 60,
                _ => 60
            };
        }

        private int GetReactivationDelay(string industry)
        {
            return industry.ToLower() switch
            {
                "barbershop" => 30,
                "lawyer" => 90,
                "spa" => 60,
                "clinic" => 180,
                "gym" => 30,
                _ => 30
            };
        }

        private ProjectBotConfigDto MapToDto(ProjectBotConfig config)
        {
            return new ProjectBotConfigDto
            {
                Id = config.Id,
                ProjectId = config.ProjectId,
                BotName = config.BotName,
                Industry = config.Industry,
                Tone = config.Tone,
                Timezone = config.Timezone,
                ReceptionConfig = JsonSerializer.Deserialize<ReceptionBotConfig>(config.ReceptionConfigJson, _jsonOptions),
                TransactionalConfig = JsonSerializer.Deserialize<TransactionalBotConfig>(config.TransactionalConfigJson, _jsonOptions),
                FeedbackConfig = JsonSerializer.Deserialize<FeedbackBotConfig>(config.FeedbackConfigJson, _jsonOptions),
                ReactivationConfig = JsonSerializer.Deserialize<ReactivationBotConfig>(config.ReactivationConfigJson, _jsonOptions),
                IntegrationsConfig = JsonSerializer.Deserialize<IntegrationsConfig>(config.IntegrationsConfigJson, _jsonOptions),
                AdvancedConfig = JsonSerializer.Deserialize<AdvancedBotConfig>(config.AdvancedConfigJson, _jsonOptions),
                IsActive = config.IsActive,
                CreatedAt = config.CreatedAt,
                UpdatedAt = config.UpdatedAt
            };
        }
    }
}
