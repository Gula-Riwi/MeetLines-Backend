using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Services;

namespace MeetLines.Application.UseCases.Services.Interfaces
{
    public interface ICreateServiceUseCase
    {
        Task<Result<ServiceDto>> ExecuteAsync(Guid userId, Guid projectId, CreateServiceRequest request, CancellationToken ct = default);
    }

    public interface IUpdateServiceUseCase
    {
        Task<Result<ServiceDto>> ExecuteAsync(Guid userId, int serviceId, UpdateServiceRequest request, CancellationToken ct = default);
    }

    public interface IDeleteServiceUseCase
    {
        Task<Result> ExecuteAsync(Guid userId, int serviceId, CancellationToken ct = default);
    }

    public interface IGetProjectServicesUseCase
    {
        Task<Result<IEnumerable<ServiceDto>>> ExecuteAsync(Guid userId, Guid projectId, CancellationToken ct = default);
    }

    public interface IGetServiceByIdUseCase
    {
        Task<Result<ServiceDto>> ExecuteAsync(Guid userId, int serviceId, CancellationToken ct = default);
    }
}
