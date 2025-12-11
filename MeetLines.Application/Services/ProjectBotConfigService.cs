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
            // Get industry defaults
            var defaults = GetIndustryDefaultsInternal(request.Industry);

            var config = new ProjectBotConfig(
                projectId: request.ProjectId,
                botName: request.BotName ?? defaults.BotName,
                industry: request.Industry,
                tone: request.Tone ?? defaults.Tone,
                timezone: request.Timezone ?? defaults.Timezone,
                receptionConfigJson: JsonSerializer.Serialize(request.ReceptionConfig ?? defaults.ReceptionConfig, _jsonOptions),
                transactionalConfigJson: JsonSerializer.Serialize(request.TransactionalConfig ?? defaults.TransactionalConfig, _jsonOptions),
                feedbackConfigJson: JsonSerializer.Serialize(request.FeedbackConfig ?? defaults.FeedbackConfig, _jsonOptions),
                reactivationConfigJson: JsonSerializer.Serialize(request.ReactivationConfig ?? defaults.ReactivationConfig, _jsonOptions),
                integrationsConfigJson: JsonSerializer.Serialize(request.IntegrationsConfig ?? defaults.IntegrationsConfig, _jsonOptions),
                advancedConfigJson: JsonSerializer.Serialize(request.AdvancedConfig ?? defaults.AdvancedConfig, _jsonOptions),
                createdBy: createdBy
            );

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

            // Update basic config if provided
            if (request.BotName != null || request.Tone != null)
            {
                config.UpdateBasicConfig(request.BotName, request.Tone, updatedBy);
            }
            
            // Update individual configurations if provided
            if (request.ReceptionConfig != null)
                config.UpdateReceptionConfig(JsonSerializer.Serialize(request.ReceptionConfig, _jsonOptions), updatedBy);
            
            if (request.TransactionalConfig != null)
                config.UpdateTransactionalConfig(JsonSerializer.Serialize(request.TransactionalConfig, _jsonOptions), updatedBy);
            
            if (request.FeedbackConfig != null)
                config.UpdateFeedbackConfig(JsonSerializer.Serialize(request.FeedbackConfig, _jsonOptions), updatedBy);
            
            if (request.ReactivationConfig != null)
                config.UpdateReactivationConfig(JsonSerializer.Serialize(request.ReactivationConfig, _jsonOptions), updatedBy);
            
            if (request.IntegrationsConfig != null)
                config.UpdateIntegrationsConfig(JsonSerializer.Serialize(request.IntegrationsConfig, _jsonOptions), updatedBy);
            
            if (request.AdvancedConfig != null)
                config.UpdateAdvancedConfig(JsonSerializer.Serialize(request.AdvancedConfig, _jsonOptions), updatedBy);

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
                    HandoffMessage = "¡Perfecto! Te ayudo con eso enseguida.",
                    OutOfHoursMessage = "Gracias por contactarnos. Nuestro horario de atención ha terminado. Te responderemos pronto."
                },
                TransactionalConfig = new TransactionalBotConfig
                {
                    Enabled = true,
                    AppointmentDurationMinutes = GetDefaultDuration(industry),
                    BufferMinutes = 0,
                    MaxAdvanceBookingDays = 30,
                    MinAdvanceBookingDays = 0,
                    ConfirmationMessage = "✅ ¡Listo! Tu cita está confirmada.",
                    SendReminder = true,
                    ReminderHoursBefore = 24,
                    ReminderMessage = "Hola, te recordamos tu cita mañana.",
                    AllowCancellation = true,
                    MinCancellationHours = 24
                },
                FeedbackConfig = new FeedbackBotConfig
                {
                    Enabled = true,
                    DelayHours = 24,
                    RequestMessage = "Hola, ¿cómo calificarías tu experiencia del 1 al 5?",
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
                        "Hola, hace días no te vemos. ¿Te gustaría agendar?",
                        "Hola, ¿cómo has estado? Tenemos disponibilidad esta semana.",
                        "Hola, te extrañamos. ¿Podemos ayudarte en algo?"
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
                "barbershop" => "¡Hola! Soy el asistente virtual. ¿Quieres agendar un corte?",
                "lawyer" => "Hola, soy el asistente virtual. ¿En qué podemos asesorarte?",
                "spa" => "¡Hola! Soy el asistente virtual. ¿Te gustaría reservar un tratamiento?",
                "clinic" => "Hola, soy el asistente virtual. ¿Necesitas agendar una consulta?",
                "gym" => "¡Hola! Soy el asistente virtual. ¿Quieres información sobre nuestras clases?",
                _ => "¡Hola! Soy el asistente virtual. ¿En qué puedo ayudarte?"
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
