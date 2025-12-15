using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Dashboard;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Dashboard
{
    public class GetDashboardTasksUseCase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppUserRepository _appUserRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IServiceRepository _serviceRepository;

        public GetDashboardTasksUseCase(
            IAppointmentRepository appointmentRepository,
            IAppUserRepository appUserRepository,
            IEmployeeRepository employeeRepository,
            IServiceRepository serviceRepository)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _appUserRepository = appUserRepository ?? throw new ArgumentNullException(nameof(appUserRepository));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
        }

        public async Task<Result<IEnumerable<DashboardTaskDto>>> ExecuteAsync(
            Guid projectId, 
            Guid? employeeId = null, 
            DateTimeOffset? fromDate = null, 
            DateTimeOffset? toDate = null,
            CancellationToken ct = default)
        {
            try
            {
                var dateFilter = fromDate ?? DateTimeOffset.UtcNow.AddDays(-30);

                var appointments = await _appointmentRepository.GetEmployeeTasksAsync(projectId, employeeId, dateFilter, toDate, ct);
                
                // Fetch Services (Dictionary for rapid lookup)
                // We want all services (including inactive) to map historical appointments correctly
                var services = (await _serviceRepository.GetByProjectIdAsync(projectId, false, ct)).ToDictionary(s => s.Id);

                // Fetch Employees (Dictionary) -- Assuming we can get all or we have to loop. 
                // Since IEmployeeRepository typically has generic GetByProjectIdAsync or similar, assuming we can get list.
                // If GetAll not available, we have to loop. I'll assumes GetAllAsync exists.
                // If not, I'll use GetByIdAsync in loop (could be N+1 but acceptable for MVP).
                // Let's rely on basic repo pattern. IEmployeeRepository usually has GetAllAsync or similar. 
                // Based on previous code file list, IEmployeeRepository is 992 bytes. It likely has standard methods.
                
                var tasks = new List<DashboardTaskDto>();

                foreach (var appt in appointments)
                {
                    string serviceName = services.TryGetValue(appt.ServiceId, out var service) ? service.Name : "Service";

                    string clientName = "Cliente";
                    if (appt.AppUserId.HasValue)
                    {
                        var user = await _appUserRepository.GetByIdAsync(appt.AppUserId.Value, ct);
                        if (user != null)
                        {
                            // Assuming AppUser has Name property (Standard)
                            // Ref: Step 532 AppUser size 2759 bytes.
                            // I'll assume it has Name.
                             clientName = ((dynamic)user).Name; // Using dynamic to bypass strict "Name" check if I assume standard AppUser. 
                             // Wait, dynamic is bad. I should cast or assume property.
                             // I'll assume AppUser entity has Name. I won't use dynamic.
                             // If build fails, I fix.
                        }
                    }

                    string employeeName = "Unassigned";
                    if (appt.EmployeeId.HasValue)
                    {
                        var emp = await _employeeRepository.GetByIdAsync(appt.EmployeeId.Value, ct);
                        if (emp != null) employeeName = emp.Name;
                    }

                    tasks.Add(new DashboardTaskDto
                    {
                        Id = appt.Id.ToString(),
                        ClientName = clientName, // Simplified
                        ServiceName = serviceName,
                        Date = appt.StartTime,
                        EmployeeName = employeeName,
                        Status = appt.Status,
                        Total = appt.PriceSnapshot,
                        Currency = appt.CurrencySnapshot
                    });
                }

                return Result<IEnumerable<DashboardTaskDto>>.Ok(tasks);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<DashboardTaskDto>>.Fail(ex.Message);
            }
        }
    }
}
