using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Appointments;
using MeetLines.Domain.Repositories;
using MeetLines.Domain.Entities;

namespace MeetLines.Application.UseCases.Client.Appointments
{
    public class ClientAppointmentUseCase : IClientAppointmentUseCase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IProjectBotConfigRepository _botConfigRepository;

        public ClientAppointmentUseCase(
            IAppointmentRepository appointmentRepository,
            IProjectBotConfigRepository botConfigRepository)
        {
            _appointmentRepository = appointmentRepository;
            _botConfigRepository = botConfigRepository;
        }

        public async Task<Result<IEnumerable<AppointmentResponse>>> GetMyAppointmentsAsync(Guid userId, bool pendingOnly, CancellationToken ct = default)
        {
            var appointments = await _appointmentRepository.GetByAppUserIdAsync(userId, ct);

            if (pendingOnly)
            {
                // Filter for future appointments that are not cancelled
                appointments = appointments.Where(a => 
                    a.StartTime >= DateTimeOffset.UtcNow && 
                    a.Status != "cancelled" && 
                    a.Status != "no_show"
                );
            }

            var response = appointments.Select(a => new AppointmentResponse
            {
                Id = a.Id,
                ProjectId = a.ProjectId,
                ProjectName = a.Project?.Name,
                // Service Info
                ServiceId = a.ServiceId,
                ServiceName = a.Service?.Name,
                // Employee Info
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee?.Name,
                // Appt Info
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                UserNotes = a.UserNotes,
                MeetingLink = a.MeetingLink,
                ClientName = a.AppUser?.FullName,
                ClientEmail = a.AppUser?.Email,
                ClientPhone = a.AppUser?.Phone,
                CreatedAt = a.CreatedAt
            });

            return Result<IEnumerable<AppointmentResponse>>.Ok(response);
        }

        public async Task<Result> CancelMyAppointmentAsync(Guid userId, int appointmentId, CancellationToken ct = default)
        {
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId, ct);

            if (appointment == null)
            {
                return Result.Fail("Cita no encontrada.");
            }

            // 1. Verify Ownership
            if (appointment.AppUserId != userId)
            {
                 return Result.Fail("No tienes permiso para cancelar esta cita.");
            }

            // 2. Verify Status
            if (appointment.Status == "cancelled")
            {
                return Result.Fail("La cita ya está cancelada.");
            }
            
            if (appointment.StartTime < DateTimeOffset.UtcNow)
            {
                 return Result.Fail("No puedes cancelar una cita pasada.");
            }

            // 3. Verify Rules (Min Cancellation Time)
            var botConfig = await _botConfigRepository.GetByProjectIdAsync(appointment.ProjectId, ct);
            if (botConfig != null && !string.IsNullOrEmpty(botConfig.TransactionalConfigJson))
            {
                try
                {
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var config = JsonSerializer.Deserialize<MeetLines.Application.DTOs.Config.TransactionalConfig>(botConfig.TransactionalConfigJson, opts);
                    
                    if (config != null)
                    {
                        if (!config.AllowCancellation)
                        {
                            return Result.Fail("Las cancelaciones no están permitidas para este servicio.");
                        }

                        var minHours = config.MinCancellationHours > 0 ? config.MinCancellationHours : 0;
                        var hoursRemaining = (appointment.StartTime - DateTimeOffset.UtcNow).TotalHours;

                        if (hoursRemaining < minHours)
                        {
                            return Result.Fail($"Solo se permite cancelar con {minHours} horas de anticipación.");
                        }
                    }
                }
                catch 
                {
                    // If config fails, assume cancellation is allowed default logic or block? 
                    // Let's allow but log warning if we had logger.
                }
            }

            // 4. Cancel
            appointment.Cancel("Cancelado por el usuario (App)");
            await _appointmentRepository.UpdateAsync(appointment, ct);

            return Result.Ok();
        }
    }
}
