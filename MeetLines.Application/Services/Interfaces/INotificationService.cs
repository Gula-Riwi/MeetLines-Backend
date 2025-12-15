using System.Threading.Tasks;

namespace MeetLines.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendAppointmentReminderAsync(int appointmentId);
        Task SendFeedbackRequestAsync(int appointmentId);
        Task SendNegativeFeedbackAlertAsync(Guid projectId, string message, CancellationToken ct = default);
        Task NotifyEmployeeOfNewChatAsync(Guid projectId, Guid employeeId, string customerPhone, CancellationToken ct = default);
    }
}
