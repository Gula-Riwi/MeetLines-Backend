using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Services;
using MeetLines.Application.UseCases.Services.Interfaces;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;

namespace MeetLines.Application.UseCases.Services
{
    public class CreateServiceUseCase : ICreateServiceUseCase
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IProjectRepository _projectRepository;

        public CreateServiceUseCase(IServiceRepository serviceRepository, IProjectRepository projectRepository)
        {
            _serviceRepository = serviceRepository;
            _projectRepository = projectRepository;
        }

        public async Task<Result<ServiceDto>> ExecuteAsync(Guid userId, Guid projectId, CreateServiceRequest request, CancellationToken ct = default)
        {
            var project = await _projectRepository.GetAsync(projectId, ct);
            if (project == null) return Result<ServiceDto>.Fail("Project not found");
            
            var service = new Service(
                projectId,
                request.Name,
                request.DurationMinutes,
                request.Price,
                request.Currency,
                request.Description
            );

            await _serviceRepository.CreateAsync(service, ct);

            return Result<ServiceDto>.Ok(MapToDto(service));
        }

        private static ServiceDto MapToDto(Service s) => new ServiceDto
        {
            Id = s.Id,
            ProjectId = s.ProjectId,
            Name = s.Name,
            Description = s.Description,
            Price = s.Price,
            Currency = s.Currency,
            DurationMinutes = s.DurationMinutes,
            IsActive = s.IsActive
        };
    }

    public class UpdateServiceUseCase : IUpdateServiceUseCase
    {
        private readonly IServiceRepository _serviceRepository;
        
        public UpdateServiceUseCase(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<Result<ServiceDto>> ExecuteAsync(Guid userId, int serviceId, UpdateServiceRequest request, CancellationToken ct = default)
        {
            var service = await _serviceRepository.GetAsync(serviceId, ct);
            if (service == null) return Result<ServiceDto>.Fail("Service not found");

            service.UpdateDetails(request.Name, request.Description, request.Price, request.DurationMinutes);
            
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value) service.Activate();
                else service.Deactivate();
            }

            await _serviceRepository.UpdateAsync(service, ct);

            return Result<ServiceDto>.Ok(MapToDto(service));
        }

        private static ServiceDto MapToDto(Service s) => new ServiceDto
        {
            Id = s.Id,
            ProjectId = s.ProjectId,
            Name = s.Name,
            Description = s.Description,
            Price = s.Price,
            Currency = s.Currency,
            DurationMinutes = s.DurationMinutes,
            IsActive = s.IsActive
        };
    }

    public class DeleteServiceUseCase : IDeleteServiceUseCase
    {
         private readonly IServiceRepository _serviceRepository;

         public DeleteServiceUseCase(IServiceRepository serviceRepository)
         {
             _serviceRepository = serviceRepository;
         }

         public async Task<Result> ExecuteAsync(Guid userId, int serviceId, CancellationToken ct = default)
         {
             var service = await _serviceRepository.GetAsync(serviceId, ct);
             if (service == null) return Result.Fail("Service not found");

             await _serviceRepository.DeleteAsync(serviceId, ct);
             return Result.Ok();
         }
    }

    public class GetProjectServicesUseCase : IGetProjectServicesUseCase
    {
        private readonly IServiceRepository _serviceRepository;

        public GetProjectServicesUseCase(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<Result<IEnumerable<ServiceDto>>> ExecuteAsync(Guid userId, Guid projectId, CancellationToken ct = default)
        {
             var services = await _serviceRepository.GetByProjectIdAsync(projectId, false, ct); // false to include inactive (admin view)
             
             var dtos = services.Select(s => new ServiceDto
             {
                Id = s.Id,
                ProjectId = s.ProjectId,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                Currency = s.Currency,
                DurationMinutes = s.DurationMinutes,
                IsActive = s.IsActive
             });

             return Result<IEnumerable<ServiceDto>>.Ok(dtos);
        }
    }

    public class GetServiceByIdUseCase : IGetServiceByIdUseCase
    {
        private readonly IServiceRepository _serviceRepository;

        public GetServiceByIdUseCase(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<Result<ServiceDto>> ExecuteAsync(Guid userId, int serviceId, CancellationToken ct = default)
        {
            var s = await _serviceRepository.GetAsync(serviceId, ct);
            if (s == null) return Result<ServiceDto>.Fail("Service not found");

            return Result<ServiceDto>.Ok(new ServiceDto
             {
                Id = s.Id,
                ProjectId = s.ProjectId,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                Currency = s.Currency,
                DurationMinutes = s.DurationMinutes,
                IsActive = s.IsActive
             });
        }
    }
}
