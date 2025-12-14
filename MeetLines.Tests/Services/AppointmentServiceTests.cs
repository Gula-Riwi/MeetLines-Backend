using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MeetLines.Application.Services;
using MeetLines.Application.Services.Interfaces;
using MeetLines.Domain.Repositories;
using MeetLines.Domain.Entities;
using MeetLines.Application.DTOs.Config;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MeetLines.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
        private readonly Mock<IServiceRepository> _mockServiceRepo;
        private readonly Mock<IEmployeeRepository> _mockEmployeeRepo;
        private readonly Mock<IProjectBotConfigRepository> _mockBotConfigRepo;
        private readonly Mock<IAppUserRepository> _mockAppUserRepo;
        private readonly Mock<IConversationRepository> _mockConversationRepo;
        private readonly Mock<ILogger<AppointmentService>> _mockLogger;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _mockAppointmentRepo = new Mock<IAppointmentRepository>();
            _mockServiceRepo = new Mock<IServiceRepository>();
            _mockEmployeeRepo = new Mock<IEmployeeRepository>();
            _mockBotConfigRepo = new Mock<IProjectBotConfigRepository>();
            _mockAppUserRepo = new Mock<IAppUserRepository>();
            _mockConversationRepo = new Mock<IConversationRepository>();
            _mockLogger = new Mock<ILogger<AppointmentService>>();

            _service = new AppointmentService(
                _mockAppointmentRepo.Object,
                _mockServiceRepo.Object,
                _mockEmployeeRepo.Object,
                _mockBotConfigRepo.Object,
                _mockAppUserRepo.Object,
                _mockConversationRepo.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetAvailableSlots_ShouldFilterPastSlots()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var today = DateTime.UtcNow.Date; // Using UTC Date for setup, but logic uses offsets
            
            // Mock Date: Today is "Today", but current time is 10:00 AM Local (-5).
            // So UtcNow should be 15:00.
            // Let's assume we request slots for "Today".
            
            // Setup Config
            var config = new TransactionalConfig
            {
                AppointmentEnabled = true,
                SlotDuration = 60,
                MinHoursBeforeBooking = 0,
                BusinessHours = new Dictionary<string, BusinessHours>
                {
                    { today.DayOfWeek.ToString().ToLower(), new BusinessHours { Start = "08:00", End = "18:00", Closed = false } }
                }
            };
            var configJson = JsonSerializer.Serialize(config);

            _mockBotConfigRepo.Setup(x => x.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProjectBotConfig(projectId, "Test Bot", "Tech", "Friendly", "UTC", "{}", configJson, "{}", "{}", "{}", "{}", Guid.NewGuid()));

            _mockEmployeeRepo.Setup(x => x.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Employee> { new Employee(projectId, "Test Emp", "testuser", "test@test.com", "hash", "role") });

            // Setup Appointments (None)
            _mockAppointmentRepo.Setup(x => x.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment>());

            // *** CRITICAL PART ***
            // We cannot easily mock DateTimeOffset.UtcNow inside the service without an abstraction (SystemClock).
            // However, the service uses `DateTimeOffset.UtcNow`.
            // To properly test this, we must rely on the fact that the test runner's verification
            // depends on the ACTUAL current time.
            
            // So if I run this test NOW, "Past Slots" are slots before NOW.
            // This makes the test flaky if run at night, but verifies the logic works "Right Now".
            
            // Let's refine the test. We want to verify that slots returned are strictly FUTURE.
            
            // Act
            // Request slots for TODAY
            var response = await _service.GetAvailableSlotsAsync(projectId, DateTime.Today);

            // Assert
            // Ensure every returned slot is in the future relative to Now (with buffer)
            var nowLocal = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5));
            
            foreach (var slot in response.Slots)
            {
                var slotTime = TimeSpan.Parse(slot.Time);
                var slotDateTime = new DateTimeOffset(DateTime.Today.Add(slotTime), TimeSpan.FromHours(-5));
                
                Assert.True(slotDateTime >= nowLocal, $"Slot {slot.Time} is in the past! Now is {nowLocal.TimeOfDay}");
            }
        }

        [Fact]
        public async Task GetAvailableSlots_ShouldRespectMinHoursBeforeBooking()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            
            // Setup Config with 2 hours lead time
            var config = new TransactionalConfig
            {
                AppointmentEnabled = true,
                MinHoursBeforeBooking = 5, // High value to ensure we filter slots
                SlotDuration = 60,
                BusinessHours = new Dictionary<string, BusinessHours>
                {
                    { DateTime.Today.DayOfWeek.ToString().ToLower(), new BusinessHours { Start = "00:00", End = "23:59", Closed = false } }
                }
            };
            var configJson = JsonSerializer.Serialize(config);

            _mockBotConfigRepo.Setup(x => x.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProjectBotConfig(projectId, "Test Bot", "Tech", "Friendly", "UTC", "{}", configJson, "{}", "{}", "{}", "{}", Guid.NewGuid()));

             _mockEmployeeRepo.Setup(x => x.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Employee> { new Employee(projectId, "Test Emp", "testuser", "test@test.com", "hash", "role") });

            _mockAppointmentRepo.Setup(x => x.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment>());

            // Act
            var response = await _service.GetAvailableSlotsAsync(projectId, DateTime.Today);

            // Assert
            var nowLocal = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5));
            var cutoff = nowLocal.AddHours(5);

            foreach (var slot in response.Slots)
            {
                var slotTime = TimeSpan.Parse(slot.Time);
                var slotDateTime = new DateTimeOffset(DateTime.Today.Add(slotTime), TimeSpan.FromHours(-5));
                
                Assert.True(slotDateTime >= cutoff, $"Slot {slot.Time} matches or exceeds 5 hour lead time. Now: {nowLocal.TimeOfDay}, Cutoff: {cutoff.TimeOfDay}");
            }
        }
    }
}
