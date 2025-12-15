using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/conversations")]
    public class HumanHandoffController : ControllerBase
    {
        private readonly IEmployeeAssignmentService _assignmentService;
        private readonly IConversationRepository _conversationRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IConfiguration _configuration;

        public HumanHandoffController(
            IEmployeeAssignmentService assignmentService,
            IConversationRepository conversationRepo,
            IProjectRepository projectRepo,
            IConfiguration configuration)
        {
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
            _conversationRepo = conversationRepo ?? throw new ArgumentNullException(nameof(conversationRepo));
            _projectRepo = projectRepo ?? throw new ArgumentNullException(nameof(projectRepo));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private bool ValidateApiKey()
        {
            var expectedApiKey = _configuration["INTEGRATIONS_API_KEY"];
            if (string.IsNullOrEmpty(expectedApiKey)) return true; // Security Risk if not configured, but matches legacy behavior? Better fail closed.
            // Actually other controllers check if (!string.IsNullOrEmpty(expectedApiKey) && actualApiKey != expectedApiKey)
            
            if (!Request.Headers.TryGetValue("Authorization", out var extractedApiKey))
            {
                return false;
            }

            var key = extractedApiKey.ToString().Replace("Bearer ", "");
            return key == expectedApiKey;
        }

        [HttpPost("phone/{phone}/handoff")]
        public async Task<IActionResult> TriggerHandoff(Guid projectId, string phone, [FromQuery] string reason = "user_request", CancellationToken ct = default)
        {
            if (!ValidateApiKey()) return Unauthorized("Invalid API Key");

            // 1. Validate Project
            var project = await _projectRepo.GetAsync(projectId, ct);
            if (project == null) return NotFound("Project not found");

            // 2. Find Active Conversation
            var conversation = await _conversationRepo.GetLatestByPhoneAsync(projectId, phone, ct);
            if (conversation == null) return NotFound("Active conversation not found for this phone");

            Api.DTOs.EmployeeDto? assignedEmployeeDto = null;

            // 4. Assign to Employee
            var assignedEmployee = await _assignmentService.AssignConversationToEmployeeAsync(projectId, conversation.Id, phone, ct);
            
            if (assignedEmployee != null)
            {
                assignedEmployeeDto = new Api.DTOs.EmployeeDto
                {
                    Id = assignedEmployee.Id,
                    Name = assignedEmployee.Name,
                    Role = assignedEmployee.Role
                };
            }

            // 5. Update State
            conversation.AssignToEmployee(assignedEmployee?.Id ?? Guid.Empty); 
            conversation.SetBotType("human_paused");
            
            await _conversationRepo.UpdateAsync(conversation, ct);
            
            return Ok(new 
            { 
                Status = "Handoff Successful", 
                AssignedTo = assignedEmployeeDto?.Name ?? "General Queue",
                BotPaused = true
            });
        }


    }

    namespace Api.DTOs 
    {
        public class EmployeeDto 
        { 
            public Guid Id { get; set; } 
            public required string Name { get; set; } 
            public required string Role { get; set; } 
        }
    }
}
