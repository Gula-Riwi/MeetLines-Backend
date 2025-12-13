using System.Threading.Tasks;

namespace MeetLines.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendAppointmentReminderAsync(int appointmentId);
    }
}
