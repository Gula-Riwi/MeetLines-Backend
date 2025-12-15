using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;
using MeetLines.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace MeetLines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId}/chat")]
    public class CustomerChatController : ControllerBase
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly INotificationService _notificationService;
        private readonly IProjectRepository _projectRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public CustomerChatController(
            IConversationRepository conversationRepository,
            INotificationService notificationService,
            IProjectRepository projectRepository,
            IEmployeeRepository employeeRepository)
        {
            _conversationRepository = conversationRepository;
            _notificationService = notificationService;
            _projectRepository = projectRepository;
            _employeeRepository = employeeRepository;
        }

        /// <summary>
        /// Gets conversation history for a specific customer phone
        /// </summary>
        [HttpGet("{phone}/history")]
        public async Task<IActionResult> GetHistory(Guid projectId, string phone, [FromQuery] int limit = 50, CancellationToken ct = default)
        {
            // TODO: Ideally we should use a proper pagination service or repo method that returns list
            // For now, let's reuse GetByCustomerPhoneAsync which returns a list of Conversation entities
            var conversations = await _conversationRepository.GetByCustomerPhoneAsync(projectId, phone, ct);
            
            // Map to simplified DTO
            var history = conversations
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .OrderBy(c => c.CreatedAt) // Return chronological order
                .Select(c => new 
                {
                    c.Id,
                    c.CustomerMessage,
                    c.BotResponse,
                    c.CreatedAt,
                    c.BotType,
                    c.HandledByHuman,
                    c.HandledByEmployeeId
                });

            return Ok(history);
        }

        /// <summary>
        /// Sends a message to the customer via WhatsApp
        /// </summary>
        [HttpPost("{phone}/send")]
        public async Task<IActionResult> SendMessage(Guid projectId, string phone, [FromBody] CustomerSendMessageRequest request, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                    return BadRequest("Message cannot be empty");

                // 1. Send via WhatsApp API
                var success = await _notificationService.SendWhatsAppMessageAsync(projectId, phone, request.Message, ct);
                
                if (!success)
                    return StatusCode(500, "Failed to send message to WhatsApp API");

                // 2. Log to Database
                var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                            ?? User.FindFirst("sub") 
                            ?? User.FindFirst("userId");

                if (claim == null || !Guid.TryParse(claim.Value, out var userId))
                {
                    throw new UnauthorizedAccessException("User ID not found or invalid in token");
                }

                var conv = new Conversation(
                    projectId: projectId,
                    customerPhone: phone,
                    customerMessage: "(Human Reply)", 
                    botResponse: request.Message,
                    botType: "human_agent",
                    customerName: null
                );
                
                // Check if User is Employee to avoid FK constraint violation
                var isEmployee = await _employeeRepository.GetByIdAsync(userId, ct) != null;
                
                if (isEmployee)
                {
                    conv.AssignToEmployee(userId);
                }
                else
                {
                    // For Admins/Owners who are not in Employees table
                    conv.MarkAsHandledByHuman();
                }
                
                await _conversationRepository.CreateAsync(conv);

                return Ok(new { success = true, conversationId = conv.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = "Internal Server Error in SendMessage", 
                    details = ex.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }
    }

    public class CustomerSendMessageRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
