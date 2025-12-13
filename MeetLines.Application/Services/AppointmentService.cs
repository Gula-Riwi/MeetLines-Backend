using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Appointments;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IProjectBotConfigRepository _botConfigRepository;
        private readonly IAppUserRepository _appUserRepository;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IServiceRepository serviceRepository,
            IEmployeeRepository employeeRepository,
            IProjectBotConfigRepository botConfigRepository,
            IAppUserRepository appUserRepository)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _botConfigRepository = botConfigRepository ?? throw new ArgumentNullException(nameof(botConfigRepository));
            _appUserRepository = appUserRepository ?? throw new ArgumentNullException(nameof(appUserRepository));
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
                CreatedAt = appointment.CreatedAt
            };

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

    // Helper classes for JSON deserialization
    public class TransactionalConfig
    {
        public bool AppointmentEnabled { get; set; }
        public Dictionary<string, BusinessHours> BusinessHours { get; set; } = new();
        public int SlotDuration { get; set; } = 30;
        public int BufferBetweenAppointments { get; set; } = 0;
    }

    public class BusinessHours
    {
        public bool Closed { get; set; }
        public string Start { get; set; } = "09:00";
        public string End { get; set; } = "18:00";
    }
}
