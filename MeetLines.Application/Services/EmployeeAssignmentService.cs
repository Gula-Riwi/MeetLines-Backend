using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace MeetLines.Application.Services
{
    public class EmployeeAssignmentService : IEmployeeAssignmentService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IConversationRepository _conversationRepo;
        private readonly INotificationService _notificationService; // For alerting the employee
        private readonly ILogger<EmployeeAssignmentService> _logger;

        public EmployeeAssignmentService(
            IEmployeeRepository employeeRepo,
            IConversationRepository conversationRepo,
            INotificationService notificationService,
            ILogger<EmployeeAssignmentService> logger)
        {
            _employeeRepo = employeeRepo ?? throw new ArgumentNullException(nameof(employeeRepo));
            _conversationRepo = conversationRepo ?? throw new ArgumentNullException(nameof(conversationRepo));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Employee?> AssignConversationToEmployeeAsync(Guid projectId, Guid conversationId, string customerPhone, CancellationToken ct = default)
        {
            // 1. Get Active Employees
            var employees = await _employeeRepo.GetActiveByProjectIdAsync(projectId, ct);
            if (!employees.Any())
            {
                _logger.LogWarning("No active employees found for Project {ProjectId}.", projectId);
                return null;
            }

            // 2. Simplified Strategy: Random (Statistically Round Robin)
            var random = new Random();
            var selectedEmployee = employees.ElementAt(random.Next(employees.Count()));

            // 3. Notify
            _logger.LogInformation("Assigned Conversation {ConversationId} to Employee {Name}", conversationId, selectedEmployee.Name);
            
            // Note: We don't update the conversation here to avoid tracking conflicts
            // The controller will handle the update after calling this method
            await _notificationService.NotifyEmployeeOfNewChatAsync(projectId, selectedEmployee.Id, customerPhone, ct);

            return selectedEmployee;
        }
    }
}
