using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Dashboard;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Dashboard
{
    public class UpdateTaskStatusUseCase
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public UpdateTaskStatusUseCase(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
        }

        public async Task<Result<bool>> ExecuteAsync(Guid projectId, int taskId, UpdateTaskStatusRequest request, CancellationToken ct = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(taskId, ct);

            if (appointment == null) 
                return Result<bool>.Fail("Task/Appointment not found");

            if (appointment.ProjectId != projectId)
                return Result<bool>.Fail("Task does not belong to this project");

            var status = request.Status.ToLowerInvariant();

            if (status == "completed")
            {
                appointment.Complete();
            }
            else if (status == "cancelled")
            {
                appointment.Cancel(request.Reason ?? "Cancelled via Dashboard");
            }
            else
            {
                return Result<bool>.Fail("Invalid status. Allowed values: completed, cancelled");
            }

            await _appointmentRepository.UpdateAsync(appointment, ct);

            return Result<bool>.Ok(true);
        }
    }
}
