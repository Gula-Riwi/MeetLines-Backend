using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Application.Services.Interfaces
{
    public interface IEmployeeAssignmentService
    {
        /// <summary>
        /// Assigns a conversation to an available employee using Round-Robin (Least Recently Assigned) logic.
        /// </summary>
        Task<Employee?> AssignConversationToEmployeeAsync(Guid projectId, Guid conversationId, string customerPhone, CancellationToken ct = default);
    }
}
