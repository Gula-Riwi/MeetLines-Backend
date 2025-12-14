using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Appointments;
using MeetLines.Application.DTOs.Services;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<Result<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken ct = default);
        Task<Result<IEnumerable<AppointmentResponse>>> GetAppointmentsAsync(Guid userId, string userRole, Guid projectId, CancellationToken ct = default);
        Task<IEnumerable<ServiceDto>> GetServicesAsync(Guid projectId, CancellationToken ct = default);
        Task<AvailableSlotsResponse> GetAvailableSlotsAsync(Guid projectId, DateTime date, int? serviceId = null, CancellationToken ct = default);
        Task<Result> UpdateAppointmentStatusAsync(int appointmentId, string newStatus, CancellationToken ct = default);
    }
}
