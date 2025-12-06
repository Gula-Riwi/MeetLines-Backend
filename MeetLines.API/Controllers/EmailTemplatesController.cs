using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/templates")]
    public class EmailTemplatesController : ControllerBase
    {
        private readonly IEmailTemplateBuilder _templateBuilder;

        public EmailTemplatesController(IEmailTemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
        }

        [HttpGet]
        public IActionResult GetAllTemplates()
        {
            var html = @"
                <html>
                <body style='font-family: Arial; padding: 20px; background: #0a192f; color: white;'>
                    <h1>Email Templates Preview</h1>
                    <ul>
                        <li><a style='color: #64ffda' href='/api/templates/verification'>Email Verification</a></li>
                        <li><a style='color: #64ffda' href='/api/templates/password-reset'>Password Reset</a></li>
                        <li><a style='color: #64ffda' href='/api/templates/welcome'>Welcome Email</a></li>
                        <li><a style='color: #64ffda' href='/api/templates/password-changed'>Password Changed</a></li>
                        <li><a style='color: #64ffda' href='/api/templates/email-verified'>Email Verified</a></li>
                        <li><a style='color: #64ffda' href='/api/templates/project-created'>Project Created</a></li>
                        <li><a style='color: #64ffda' href='/api/templates/employee-credentials'>Employee Credentials</a></li>
                        <li><a style='color: #64ffda' href='/api/templates/appointment-assigned'>Appointment Assigned</a></li>
                        <li><a style='color: #64ffda' href='/api/templates/appointment-confirmed'>Appointment Confirmed</a></li>
                        <li><a style='color: #64ffda' href='/api/templates/appointment-cancelled'>Appointment Cancelled</a></li>
                    </ul>
                </body>
                </html>";
            return Content(html, "text/html");
        }

        [HttpGet("{type}")]
        public IActionResult GetTemplate(string type)
        {
            string content = "";
            try
            {
                content = type.ToLower() switch
                {
                    "verification" => _templateBuilder.BuildEmailVerification("John Doe", "http://localhost:3000/verify?token=xyz"),
                    "password-reset" => _templateBuilder.BuildPasswordReset("John Doe", "http://localhost:3000/reset?token=xyz"),
                    "welcome" => _templateBuilder.BuildWelcome("John Doe", "http://localhost:3000/login"),
                    "password-changed" => _templateBuilder.BuildPasswordChanged("John Doe", "http://localhost:3000/login"),
                    "email-verified" => _templateBuilder.BuildEmailVerified("John Doe", "http://localhost:3000/dashboard"),
                    "project-created" => _templateBuilder.BuildProjectCreated("John Doe", "My Awesome SaaS"),
                    "employee-credentials" => _templateBuilder.BuildEmployeeCredentials("Jane Smith", "jane.smith", "Xy7#kL9p", "Sales"),
                    "appointment-assigned" => _templateBuilder.BuildAppointmentAssigned("Jane Agent", "Bob Client", DateTime.Now.AddDays(1), "10:00 AM"),
                    "appointment-confirmed" => _templateBuilder.BuildAppointmentConfirmed("Bob Client", "Jane Agent", DateTime.Now.AddDays(1), "10:00 AM"),
                    "appointment-cancelled" => _templateBuilder.BuildAppointmentCancelled("John Doe", DateTime.Now, "10:00 AM", "Conflict with another meeting"),
                    _ => "<h1>Template not found</h1>"
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Content(content, "text/html");
        }
    }
}
