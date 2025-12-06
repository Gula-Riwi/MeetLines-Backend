using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Appointments;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentAssignmentService _assignmentService;
        private readonly IEmailService _emailService;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ISaasUserRepository _userRepository; 
        private readonly IProjectRepository _projectRepository; // Added

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IAppointmentAssignmentService assignmentService,
            IEmailService emailService,
            IEmployeeRepository employeeRepository,
            ISaasUserRepository userRepository,
            IProjectRepository projectRepository) // Added
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository)); // Added
        }

        public async Task<Result<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken ct = default)
        {
            try
            {
                // Get Project Info for Email Sender Name
                var project = await _projectRepository.GetAsync(request.ProjectId, ct);
                var senderName = project?.Name ?? "MeetLines";

                // 1. Assign Employee
                // Use a default area "General" or allow passing area in request. 
                // For MVP, let's assume "General" or handle if service has an area concept.
                // Request doesn't have Area yet. Let's assume General for now.
                var area = "General"; 
                var assignedEmployee = await _assignmentService.FindAvailableEmployeeAsync(request.ProjectId, area, ct);
                
                // 2. Create Appointment Entity
                // NOTE: 'AppUserId' is required by entity. In this flow, we might be efficiently creating a user 
                // or using a guest ID. The Entity requires GUID. 
                // Let's generate a placeholder GUID for the guest client if they are not logged in.
                // ideally we should create a 'Lead' or 'User' for them.
                var appUserId = Guid.NewGuid(); // Placeholder for guest
                
                // PriceSnapshot defaults to 0 for now as we don't have Service repo to fetch price yet.
                var price = 0m; 

                var appointment = new Appointment(
                    request.ProjectId,
                    null, // LeadId
                    appUserId,
                    request.ServiceId,
                    request.StartTime,
                    request.EndTime,
                    price,
                    "COP",
                    request.UserNotes
                );

                if (assignedEmployee != null)
                {
                    appointment.AssignToEmployee(assignedEmployee.Id);
                }

                // Confirm immediately for MVP "Wow" factor
                appointment.Confirm("http://meetlines.com/meet/" + appointment.Id);

                // 3. Save to DB
                await _appointmentRepository.AddAsync(appointment, ct);

                // 4. Send Notifications
                // To Client
                await _emailService.SendAppointmentConfirmedAsync(
                    request.ClientEmail, 
                    request.ClientName, 
                    assignedEmployee?.Name ?? "MeetLines Staff", 
                    request.StartTime.Date, 
                    request.StartTime.ToString("HH:mm"),
                    senderName // Pass sender name
                );

                // To Employee
                if (assignedEmployee != null)
                {
                    // Assuming Username is Email for Employee or we need to add Email to Employee entity.
                    // IMPORTANT: We previously assumed Username is used for login. 
                    // Let's assume Username IS the email for notification purposes.
                    await _emailService.SendAppointmentAssignedAsync(
                        assignedEmployee.Email, 
                        assignedEmployee.Name, 
                        request.ClientName, 
                        request.StartTime.Date, 
                        request.StartTime.ToString("HH:mm"),
                        senderName // Pass sender name
                    );
                }

                return Result<AppointmentResponse>.Ok(MapToResponse(appointment, assignedEmployee?.Name));
            }
            catch (Exception ex)
            {
                // Log exception
                return Result<AppointmentResponse>.Fail($"Error creating appointment: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<AppointmentResponse>>> GetAppointmentsAsync(Guid userId, string userRole, Guid projectId, CancellationToken ct = default)
        {
            try
            {
                IEnumerable<Appointment> appointments;

                // Validate access rights
                // Simplification: We trust the controller to pass correct Role/ProjectId context
                
                if (userRole == "Employee")
                {
                    // Employees see only their own appointments
                    // Verify that the userId effectively belongs to this Project? 
                    // The query filters by EmployeeId=userId, so they can't see others anyway.
                    appointments = await _appointmentRepository.GetByEmployeeIdAsync(userId, ct);
                }
                else 
                {
                    // Admin/Owner sees all appointments for the project
                    // 'User' role usually implies Owner in this context, or we check if user is owner of project.
                    // For now, assume if not Employee, and has ProjectId, they see all.
                    appointments = await _appointmentRepository.GetByProjectIdAsync(projectId, ct);
                }

                // Map to DTO
                var responses = new List<AppointmentResponse>();
                foreach (var app in appointments)
                {
                    string? empName = null;
                    if (app.EmployeeId.HasValue)
                    {
                        var emp = await _employeeRepository.GetByIdAsync(app.EmployeeId.Value, ct);
                        empName = emp?.Name;
                    }
                    responses.Add(MapToResponse(app, empName));
                }

                return Result<IEnumerable<AppointmentResponse>>.Ok(responses);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<AppointmentResponse>>.Fail($"Error retrieving appointments: {ex.Message}");
            }
        }

        private AppointmentResponse MapToResponse(Appointment app, string? employeeName)
        {
            return new AppointmentResponse
            {
                Id = app.Id,
                ProjectId = app.ProjectId,
                ServiceId = app.ServiceId,
                EmployeeId = app.EmployeeId,
                EmployeeName = employeeName,
                StartTime = app.StartTime,
                EndTime = app.EndTime,
                Status = app.Status,
                UserNotes = app.UserNotes,
                CreatedAt = app.CreatedAt
            };
        }
    }
}
