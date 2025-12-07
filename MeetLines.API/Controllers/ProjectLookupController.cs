using Microsoft.AspNetCore.Mvc;
using MeetLines.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MeetLines.API.Controllers
{
    /// <summary>
    /// Controller for project lookup (for n8n and external integrations)
    /// Requires API key authentication
    /// </summary>
    [ApiController]
    [Route("api/projects")]
    public class ProjectLookupController : ControllerBase
    {
        private readonly MeetLinesPgDbContext _context;
        private readonly Microsoft.Extensions.Logging.ILogger<ProjectLookupController> _logger;
        private readonly string _apiKey;

        public ProjectLookupController(
            MeetLinesPgDbContext context,
            Microsoft.Extensions.Logging.ILogger<ProjectLookupController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiKey = Environment.GetEnvironmentVariable("INTEGRATIONS_API_KEY") ?? string.Empty;
        }

        /// <summary>
        /// Gets project by WhatsApp Phone Number ID (from Meta/Facebook)
        /// Used by n8n to identify which project a message belongs to
        /// Requires header: Authorization: Bearer {INTEGRATIONS_API_KEY}
        /// </summary>
        [HttpGet("by-whatsapp-id/{phoneNumberId}")]
        public async Task<ActionResult> GetByWhatsAppPhoneNumberId(string phoneNumberId, CancellationToken ct = default)
        {
            try
            {
                // Validate API key
                if (!ValidateApiKey())
                {
                    _logger.LogWarning("Unauthorized access attempt to project lookup by WhatsApp ID");
                    return Unauthorized(new { error = "Invalid or missing API key" });
                }

                // Query directly without tenant filter
                var project = await _context.Projects
                    .AsNoTracking()
                    .IgnoreQueryFilters() // IMPORTANTE: Ignora el filtro de tenant
                    .FirstOrDefaultAsync(p => p.WhatsappPhoneNumberId == phoneNumberId, ct);

                if (project == null)
                {
                    _logger.LogWarning("Project not found for WhatsApp Phone Number ID: {PhoneNumberId}", phoneNumberId);
                    return NotFound(new { message = $"No project found for WhatsApp Phone Number ID: {phoneNumberId}" });
                }

                _logger.LogInformation("Project {ProjectId} found for WhatsApp Phone Number ID: {PhoneNumberId}", project.Id, phoneNumberId);

                return Ok(new
                {
                    projectId = project.Id,
                    projectName = project.Name,
                    subdomain = project.Subdomain,
                    industry = project.Industry,
                    whatsappPhoneNumberId = project.WhatsappPhoneNumberId,
                    userId = project.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project by WhatsApp Phone Number ID: {PhoneNumberId}", phoneNumberId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Gets project by subdomain
        /// Alternative method for project lookup
        /// Requires header: Authorization: Bearer {INTEGRATIONS_API_KEY}
        /// </summary>
        [HttpGet("by-subdomain/{subdomain}")]
        public async Task<ActionResult> GetBySubdomain(string subdomain, CancellationToken ct = default)
        {
            try
            {
                // Validate API key
                if (!ValidateApiKey())
                {
                    _logger.LogWarning("Unauthorized access attempt to project lookup by subdomain");
                    return Unauthorized(new { error = "Invalid or missing API key" });
                }

                // Query directly without tenant filter
                var project = await _context.Projects
                    .AsNoTracking()
                    .IgnoreQueryFilters() // IMPORTANTE: Ignora el filtro de tenant
                    .FirstOrDefaultAsync(p => p.Subdomain == subdomain, ct);
                
                if (project == null)
                {
                    _logger.LogWarning("Project not found for subdomain: {Subdomain}", subdomain);
                    return NotFound(new { message = $"No project found for subdomain: {subdomain}" });
                }

                _logger.LogInformation("Project {ProjectId} found for subdomain: {Subdomain}", project.Id, subdomain);

                return Ok(new
                {
                    projectId = project.Id,
                    projectName = project.Name,
                    subdomain = project.Subdomain,
                    industry = project.Industry,
                    whatsappPhoneNumberId = project.WhatsappPhoneNumberId,
                    userId = project.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project by subdomain: {Subdomain}", subdomain);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Gets only the project ID by WhatsApp Phone Number ID (lightweight)
        /// Used by n8n for optimized lookups
        /// Requires header: Authorization: Bearer {INTEGRATIONS_API_KEY}
        /// </summary>
        [HttpGet("by-whatsapp-id/{phoneNumberId}/id")]
        public async Task<ActionResult> GetProjectIdOnly(string phoneNumberId, CancellationToken ct = default)
        {
            try
            {
                // Validate API key
                if (!ValidateApiKey())
                {
                    _logger.LogWarning("Unauthorized access attempt to project ID lookup");
                    return Unauthorized(new { error = "Invalid or missing API key" });
                }

                // Query directly without tenant filter
                var project = await _context.Projects
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.WhatsappPhoneNumberId == phoneNumberId, ct);

                if (project == null)
                {
                    _logger.LogWarning("Project not found for WhatsApp Phone Number ID: {PhoneNumberId}", phoneNumberId);
                    return NotFound(new { message = $"No project found for WhatsApp Phone Number ID: {phoneNumberId}" });
                }

                _logger.LogInformation("Project ID {ProjectId} found for WhatsApp Phone Number ID: {PhoneNumberId}", project.Id, phoneNumberId);

                return Ok(new
                {
                    projectId = project.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project ID by WhatsApp Phone Number ID: {PhoneNumberId}", phoneNumberId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Health check endpoint for n8n
        /// Does not require authentication
        /// </summary>
        [HttpGet("lookup/health")]
        public ActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                service = "project-lookup",
                timestamp = DateTime.UtcNow
            });
        }

        private bool ValidateApiKey()
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogError("INTEGRATIONS_API_KEY not configured in environment variables");
                return false;
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                return false;
            }

            var parts = authHeader.Split(' ');
            if (parts.Length != 2 || !string.Equals(parts[0], "Bearer", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return string.Equals(parts[1], _apiKey, StringComparison.Ordinal);
        }
    }
}
