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

        public CustomerChatController(
            IConversationRepository conversationRepository,
            INotificationService notificationService,
            IProjectRepository projectRepository)
        {
            _conversationRepository = conversationRepository;
            _notificationService = notificationService;
            _projectRepository = projectRepository;
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
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message cannot be empty");

            // 1. Send via WhatsApp API
            var success = await _notificationService.SendWhatsAppMessageAsync(projectId, phone, request.Message, ct);
            
            if (!success)
                return StatusCode(500, "Failed to send message to WhatsApp API");

            // 2. Log to Database (Create a Conversation record representing this outbound message)
            // We store it as a conversation where CustomerMessage is potentially empty/system or we use a specific convention?
            // Actually, Conversation entity structure is Customer Says -> Bot Responds.
            // For Human -> Customer, it's slightly different. 
            // We can treat Employee as "Bot" in the schema context or we need a Message entity.
            // Given current schema constraints, we will create a Conversation record where:
            // CustomerMessage = "(Employee Reply)" or similar marker
            // BotResponse = Actual Employee Message
            // BotType = "human_agent"
            
            var employeeId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

            var conv = new Conversation(
                projectId: projectId,
                customerPhone: phone,
                customerMessage: "(Human Reply)", // Marker
                botResponse: request.Message,
                botType: "human_agent",
                customerName: null // Optional
            );
            
            conv.AssignToEmployee(employeeId);
            
            await _conversationRepository.CreateAsync(conv);

            return Ok(new { success = true, conversationId = conv.Id });
        }
    }

    public class CustomerSendMessageRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
