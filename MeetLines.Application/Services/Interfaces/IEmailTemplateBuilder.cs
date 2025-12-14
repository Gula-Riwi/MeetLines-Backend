using System;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IEmailTemplateBuilder
    {
        string BuildEmailVerification(string userName, string verificationUrl);
        string BuildPasswordReset(string userName, string resetUrl);
        string BuildWelcome(string userName, string loginUrl);
        string BuildPasswordChanged(string userName, string loginUrl);
        string BuildEmailVerified(string userName, string dashboardUrl);
        string BuildProjectCreated(string userName, string projectName);
        string BuildEmployeeCredentials(string name, string username, string password, string area);
        string BuildAppointmentAssigned(string employeeName, string clientName, DateTime date, string time);
        string BuildAppointmentConfirmed(string clientName, string employeeName, DateTime date, string time);
        string BuildAppointmentCancelled(string userName, DateTime date, string time, string reason);
        string BuildNegativeFeedbackAlert(string ownerName, string customerName, string customerPhone, int rating, string comment, string projectName);
    }
}
