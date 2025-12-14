using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Employees;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IProjectRepository _projectRepository;
        private readonly IEmailService _emailService;
        private readonly ITenantService _tenantService;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            IPasswordHasher passwordHasher,
            IProjectRepository projectRepository,
            IEmailService emailService,
            ITenantService tenantService)
        {
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        }

        public async Task<Result<EmployeeResponse>> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken ct = default)
        {
            try
            {
                // Validate Project existence
                var project = await _projectRepository.GetAsync(request.ProjectId, ct);
                if (project == null)
                {
                    return Result<EmployeeResponse>.Fail("Proyecto no encontrado");
                }

                // Validate tenant consistency (subdomain match)
                var currentSubdomain = _tenantService.GetCurrentSubdomain();
                if (!string.IsNullOrWhiteSpace(currentSubdomain) &&
                    !string.Equals(project.Subdomain, currentSubdomain, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<EmployeeResponse>.Fail("El proyecto no pertenece al tenant actual.");
                }

                // Auto‑generate username from email if not provided
                string username = request.Username ?? string.Empty;
                if (string.IsNullOrWhiteSpace(username))
                {
                    username = request.Email.Split('@')[0];
                    var baseUsername = username;
                    int suffix = 1;
                    while (await _employeeRepository.GetByUsernameAsync(username, ct) != null)
                    {
                        username = $"{baseUsername}{suffix}";
                        suffix++;
                    }
                }
                else
                {
                    var existing = await _employeeRepository.GetByUsernameAsync(username, ct);
                    if (existing != null)
                    {
                        return Result<EmployeeResponse>.Fail("El nombre de usuario ya está en uso");
                    }
                }

                // Generate random password
                var password = GenerateRandomPassword();
                var passwordHash = _passwordHasher.HashPassword(password);

                var employee = new Employee(request.ProjectId, request.Name, username, request.Email, passwordHash, request.Role, request.Area);
                await _employeeRepository.AddAsync(employee, ct);

                // Send credentials via email
                await _emailService.SendEmployeeCredentialsAsync(request.Email, request.Name, username, password, request.Area);

                return Result<EmployeeResponse>.Ok(MapToResponse(employee));
            }
            catch (Exception ex)
            {
                return Result<EmployeeResponse>.Fail($"Error al crear empleado: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<EmployeeResponse>>> GetEmployeesByProjectAsync(Guid projectId, CancellationToken ct = default)
        {
            try
            {
                var employees = await _employeeRepository.GetByProjectIdAsync(projectId, ct);
                var responses = employees.Select(MapToResponse);
                return Result<IEnumerable<EmployeeResponse>>.Ok(responses);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<EmployeeResponse>>.Fail($"Error al obtener empleados: {ex.Message}");
            }
        }

        public async Task<Result<EmployeeResponse>> UpdateEmployeeAsync(Guid employeeId, UpdateEmployeeRequest request, CancellationToken ct = default)
        {
            try 
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId, ct);
                if (employee == null) return Result<EmployeeResponse>.Fail("Empleado no encontrado");

                employee.UpdateDetails(request.Name, request.Role, request.Area ?? string.Empty);
                await _employeeRepository.UpdateAsync(employee, ct);

                return Result<EmployeeResponse>.Ok(MapToResponse(employee));
            }
            catch (Exception ex)
            {
                 return Result<EmployeeResponse>.Fail($"Error al actualizar empleado: {ex.Message}");
            }
        }

        public async Task<Result> ToggleEmployeeStatusAsync(Guid employeeId, bool isActive, CancellationToken ct = default)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId, ct);
                if (employee == null) return Result.Fail("Empleado no encontrado");

                if (isActive) employee.Activate();
                else employee.Deactivate();

                await _employeeRepository.UpdateAsync(employee, ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al cambiar estado: {ex.Message}");
            }
        }

        private static EmployeeResponse MapToResponse(Employee e)
        {
            return new EmployeeResponse
            {
                Id = e.Id,
                ProjectId = e.ProjectId,
                Name = e.Name,
                Username = e.Username,
                Email = e.Email,
                Role = e.Role,
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt
            };
        }

        private static string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
