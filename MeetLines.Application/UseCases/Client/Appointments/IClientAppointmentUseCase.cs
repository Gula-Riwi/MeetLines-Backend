using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Appointments;

namespace MeetLines.Application.UseCases.Client.Appointments
{
    public interface IClientAppointmentUseCase
    {
        Task<Result<IEnumerable<AppointmentResponse>>> GetMyAppointmentsAsync(Guid userId, bool pendingOnly, CancellationToken ct = default);
        Task<Result> CancelMyAppointmentAsync(Guid userId, int appointmentId, CancellationToken ct = default);
    }
}
