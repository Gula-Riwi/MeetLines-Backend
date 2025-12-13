using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Appointments;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Application.DTOs.Services;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Application.DTOs.Config; // Unified DTO
using Microsoft.Extensions.Logging;

namespace MeetLines.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IProjectBotConfigRepository _botConfigRepository;
        private readonly IAppUserRepository _appUserRepository;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IServiceRepository serviceRepository,
            IEmployeeRepository employeeRepository,
            IProjectBotConfigRepository botConfigRepository,
            IAppUserRepository appUserRepository,
            ILogger<AppointmentService> logger)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _botConfigRepository = botConfigRepository ?? throw new ArgumentNullException(nameof(botConfigRepository));
            _appUserRepository = appUserRepository ?? throw new ArgumentNullException(nameof(appUserRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ServiceDto>> GetServicesAsync(Guid projectId, CancellationToken ct = default)
        {
            var services = await _serviceRepository.GetByProjectIdAsync(projectId, true, ct);
            return services.Select(s => new ServiceDto
            {
                Id = s.Id,
                ProjectId = s.ProjectId,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                Currency = s.Currency,
                DurationMinutes = s.DurationMinutes,
                IsActive = s.IsActive
            });
        }

        public async Task<AvailableSlotsResponse> GetAvailableSlotsAsync(Guid projectId, DateTime date, int? serviceId = null, CancellationToken ct = default)
        {
            // 1. Get BotConfig
            var botConfig = await _botConfigRepository.GetByProjectIdAsync(projectId, ct);
            if (botConfig == null || string.IsNullOrEmpty(botConfig.TransactionalConfigJson))
            {
                return new AvailableSlotsResponse { Date = date.ToString("yyyy-MM-dd"), Slots = new List<AvailableSlotDto>() };
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config = JsonSerializer.Deserialize<TransactionalConfig>(botConfig.TransactionalConfigJson, options);
            if (config == null || !config.AppointmentEnabled)
            {
                return new AvailableSlotsResponse { Date = date.ToString("yyyy-MM-dd"), Slots = new List<AvailableSlotDto>() };
            }

            // 2. Get business hours for the day
            var dayOfWeek = date.DayOfWeek.ToString().ToLower();
            if (!config.BusinessHours.ContainsKey(dayOfWeek))
            {
                return new AvailableSlotsResponse { Date = date.ToString("yyyy-MM-dd"), Slots = new List<AvailableSlotDto>() };
            }

            var businessHours = config.BusinessHours[dayOfWeek];
            if (businessHours.Closed)
            {
                return new AvailableSlotsResponse { Date = date.ToString("yyyy-MM-dd"), Slots = new List<AvailableSlotDto>() };
            }

            // 3. Get active employees
            var allEmployees = await _employeeRepository.GetByProjectIdAsync(projectId, ct);
            var employees = allEmployees.Where(e => e.IsActive).ToList();
            if (!employees.Any())
            {
                return new AvailableSlotsResponse { Date = date.ToString("yyyy-MM-dd"), Slots = new List<AvailableSlotDto>() };
            }

            // 4. Get existing appointments for the date
            var allAppointments = await _appointmentRepository.GetByProjectIdAsync(projectId, ct);
            var dateStart = new DateTimeOffset(date.Date, TimeSpan.Zero);
            var dateEnd = dateStart.AddDays(1);
            var existingAppointments = allAppointments.Where(a => 
                a.StartTime >= dateStart && a.StartTime < dateEnd).ToList();

            // 5. Calculate available slots
            var slots = new List<AvailableSlotDto>();
            
            if (!TimeSpan.TryParse(businessHours.Start, out var startTime) || 
                !TimeSpan.TryParse(businessHours.End, out var endTime))
            {
                return new AvailableSlotsResponse { Date = date.ToString("yyyy-MM-dd"), Slots = new List<AvailableSlotDto>() };
            }
            
            var slotDuration = config.SlotDuration;

            foreach (var employee in employees)
            {
                var currentTime = startTime;

                while (currentTime.Add(TimeSpan.FromMinutes(slotDuration)) <= endTime)
                {
                    var slotStart = new DateTimeOffset(date.Date.Add(currentTime), TimeSpan.Zero);
                    var slotEnd = slotStart.AddMinutes(slotDuration);

                    // Check if employee is available
                    var isAvailable = !existingAppointments.Any(a =>
                        a.EmployeeId == employee.Id &&
                        a.Status != "cancelled" &&
                        a.StartTime < slotEnd &&
                        a.EndTime > slotStart);

                    if (isAvailable)
                    {
                        slots.Add(new AvailableSlotDto
                        {
                            Time = currentTime.ToString(@"hh\:mm"),
                            EmployeeId = employee.Id,
                            EmployeeName = employee.Name,
                            EmployeeRole = employee.Role
                        });
                    }

                    currentTime = currentTime.Add(TimeSpan.FromMinutes(slotDuration));
                }
            }

            return new AvailableSlotsResponse
            {
                Date = date.ToString("yyyy-MM-dd"),
                Slots = slots
            };
        }

        public async Task<Result<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken ct = default)
        {
            // Get service details
            var service = await _serviceRepository.GetAsync(request.ServiceId, ct);
            if (service == null)
            {
                return Result<AppointmentResponse>.Fail($"Service {request.ServiceId} not found");
            }

            Employee? employee = null;
            // Validate Employee if provided
            if (request.EmployeeId.HasValue)
            {
                employee = await _employeeRepository.GetByIdAsync(request.EmployeeId.Value, ct);
                if (employee == null)
                {
                     return Result<AppointmentResponse>.Fail($"Employee {request.EmployeeId} not found");
                }
            }


            // Get or create AppUser
            var appUser = await _appUserRepository.GetByEmailAsync(request.ClientEmail, ct);
            if (appUser == null)
            {
                // Create new AppUser for this customer
                appUser = new AppUser(
                    email: request.ClientEmail,
                    fullName: request.ClientName,
                    phone: request.ClientPhone, 
                    authProvider: "bot"
                );
                await _appUserRepository.AddAsync(appUser, ct);
            }
            else if (!string.IsNullOrEmpty(request.ClientPhone) && string.IsNullOrEmpty(appUser.Phone))
            {
                // Update phone if missing
                appUser.UpdateInfo(appUser.FullName, request.ClientPhone);
                await _appUserRepository.UpdateAsync(appUser, ct);
            }


            // === IDEMPOTENCY CHECK ===
            // Npgsql 6.0+ requires UTC for DateTimeOffset comparisons
            var searchTimeUtc = request.StartTime.ToUniversalTime();
            var existingAppt = await _appointmentRepository.FindDuplicateAsync(request.ProjectId, appUser.Id, searchTimeUtc, ct);
            if (existingAppt != null)
            {
                _logger.LogInformation($"Idempotency: Returning existing appointment {existingAppt.Id} for User {appUser.Id} at {request.StartTime}.");
                return Result<AppointmentResponse>.Ok(new AppointmentResponse
                {
                    Id = existingAppt.Id,
                    ProjectId = existingAppt.ProjectId,
                    ServiceId = existingAppt.ServiceId,
                    EmployeeId = existingAppt.EmployeeId,
                    StartTime = existingAppt.StartTime,
                    EndTime = existingAppt.EndTime,
                    Status = existingAppt.Status,
                    ClientName = appUser.FullName,
                    ClientEmail = appUser.Email,
                    ClientPhone = appUser.Phone,
                    MeetingLink = existingAppt.MeetingLink
                });
            }
            // =========================

            // Create appointment with the AppUser
            var appointment = new Appointment(
                projectId: request.ProjectId,
                leadId: null,
                appUserId: appUser.Id,
                serviceId: request.ServiceId,
                startTime: request.StartTime,
                endTime: request.EndTime,
                priceSnapshot: service.Price,
                currencySnapshot: service.Currency,
                userNotes: request.UserNotes
            );

            if (request.EmployeeId.HasValue)
            {
                appointment.AssignToEmployee(request.EmployeeId.Value);
            }

            await _appointmentRepository.AddAsync(appointment, ct);

            var response = new AppointmentResponse
            {
                Id = appointment.Id,
                ProjectId = appointment.ProjectId,
                ServiceId = appointment.ServiceId,
                EmployeeId = appointment.EmployeeId,
                EmployeeName = employee?.Name,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                UserNotes = appointment.UserNotes,
                ClientName = appUser.FullName,
                ClientEmail = appUser.Email,
                ClientPhone = appUser.Phone,
                MeetingLink = appointment.MeetingLink,
                CreatedAt = appointment.CreatedAt
            };

            // === HANGFIRE SCHEDULING ===
            try 
            {
                // 1. Get Config to check if reminders are enabled
                var botConfig = await _botConfigRepository.GetByProjectIdAsync(request.ProjectId, ct);
                
                // DEBUG LOGGING
                if (botConfig == null) _logger.LogWarning($"[Hangfire] BotConfig is NULL for Project {request.ProjectId}");
                else _logger.LogWarning($"[Hangfire] BotConfig found. JSON: {botConfig.TransactionalConfigJson}");

                if (botConfig != null && !string.IsNullOrEmpty(botConfig.TransactionalConfigJson))
                {
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var trans = JsonSerializer.Deserialize<MeetLines.Application.DTOs.Config.TransactionalConfig>(botConfig.TransactionalConfigJson, opts);
                    
                    _logger.LogWarning($"[Hangfire] Deserialized Config: Enabled={trans?.AppointmentEnabled}, SendReminder={trans?.SendReminder}, Hours={trans?.ReminderHoursBefore}");

                    if (trans != null && trans.AppointmentEnabled && trans.SendReminder)
                    {
                        var hoursBefore = trans.ReminderHoursBefore > 0 ? trans.ReminderHoursBefore : 24;
                        var reminderTime = request.StartTime.AddHours(-hoursBefore);

                        if (reminderTime > DateTimeOffset.UtcNow)
                        {
                            // Schedule Job using Hangfire
                            var jobId = Hangfire.BackgroundJob.Schedule<INotificationService>(
                                service => service.SendAppointmentReminderAsync(appointment.Id),
                                reminderTime
                            );
                            _logger.LogWarning($"[Hangfire] SUCCESS. Scheduled Job {jobId} for Appt {appointment.Id} at {reminderTime} (UTC)");
                        }
                        else
                        {
                            _logger.LogWarning($"[Hangfire] SKIPPED. Reminder Time {reminderTime} is in the past. Now is {DateTimeOffset.UtcNow}.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"[Hangfire] DISABLED. Conditions verified: ApptEnabled={trans?.AppointmentEnabled}, SendReminder={trans?.SendReminder}");
                    }
                }
                else
                {
                   _logger.LogWarning($"[Hangfire] SKIPPED. Config JSON is empty."); 
                }
            }
            catch (Exception ex) 
            { 
                 _logger.LogError(ex, $"Failed to schedule Hangfire reminder for Appointment {appointment.Id}");
            }
            // ===========================

            return Result<AppointmentResponse>.Ok(response);
        }

        public async Task<Result<IEnumerable<AppointmentResponse>>> GetAppointmentsAsync(Guid userId, string userRole, Guid projectId, CancellationToken ct = default)
        {
            var appointments = await _appointmentRepository.GetByProjectIdAsync(projectId, ct);
            
            var responses = appointments.Select(a => new AppointmentResponse
            {
                Id = a.Id,
                ProjectId = a.ProjectId,
                ServiceId = a.ServiceId,
                EmployeeId = a.EmployeeId,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                UserNotes = a.UserNotes,
                CreatedAt = a.CreatedAt
            });

            return Result<IEnumerable<AppointmentResponse>>.Ok(responses);
        }
    }
}
