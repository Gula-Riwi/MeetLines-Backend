using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.BotSystem;
using MeetLines.Application.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId}/knowledge-base")]
    public class KnowledgeBaseController : ControllerBase
    {
        private readonly IKnowledgeBaseService _service;
        private readonly IConfiguration _configuration;

        public KnowledgeBaseController(IKnowledgeBaseService service, IConfiguration configuration)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private bool ValidateApiKey()
        {
            var expectedApiKey = _configuration["INTEGRATIONS_API_KEY"];
            if (string.IsNullOrEmpty(expectedApiKey))
            {
                return false;
            }

            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return false;
            }

            var token = authHeader.ToString().Replace("Bearer ", "");
            return token == expectedApiKey;
        }

        /// <summary>
        /// Gets all knowledge base entries for a project
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAll(Guid projectId, [FromQuery] bool activeOnly = true, CancellationToken ct = default)
        {
            var entries = await _service.GetByProjectIdAsync(projectId, activeOnly, ct);
            return Ok(entries);
        }

        /// <summary>
        /// Searches knowledge base and returns ONLY the best match
        /// Used by n8n - requires API key authentication
        /// </summary>
        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<ActionResult> Search(Guid projectId, [FromBody] SearchKnowledgeBaseRequest request, CancellationToken ct = default)
        {
            // Validate API key for n8n integration
            if (!ValidateApiKey())
            {
                return Unauthorized(new { error = "Invalid or missing API key" });
            }

            if (request.ProjectId != projectId)
            {
                return BadRequest(new { message = "Project ID mismatch" });
            }

            // Return only the best match to avoid multiple n8n executions
            var result = await _service.SearchBestAsync(request, ct);
            
            if (result == null)
            {
                // Return empty object when no match found
                return Ok(new 
                { 
                    answer = (string?)null,
                    question = (string?)null,
                    category = (string?)null,
                    message = "No se encontró información relevante" 
                });
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Creates a knowledge base entry
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<KnowledgeBaseDto>> Create(
            Guid projectId,
            [FromBody] CreateKnowledgeBaseRequest request,
            CancellationToken ct = default)
        {
            if (request.ProjectId != projectId)
            {
                return BadRequest(new { message = "Project ID mismatch" });
            }

            var entry = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetAll), new { projectId }, entry);
        }

        /// <summary>
        /// Updates a knowledge base entry
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<KnowledgeBaseDto>> Update(
            Guid projectId,
            Guid id,
            [FromBody] UpdateKnowledgeBaseRequest request,
            CancellationToken ct = default)
        {
            var entry = await _service.UpdateAsync(id, request, ct);
            return Ok(entry);
        }

        /// <summary>
        /// Deletes a knowledge base entry
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid projectId, Guid id, CancellationToken ct = default)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Increments usage count (called by n8n when entry is used)
        /// </summary>
        [HttpPost("{id}/increment-usage")]
        [AllowAnonymous] // n8n webhook
        public async Task<ActionResult> IncrementUsage(Guid projectId, Guid id, CancellationToken ct = default)
        {
            await _service.IncrementUsageAsync(id, ct);
            return Ok();
        }
    }
}
