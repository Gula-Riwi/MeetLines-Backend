using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct = default);
        Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(Payment payment, CancellationToken ct = default);
    }
}