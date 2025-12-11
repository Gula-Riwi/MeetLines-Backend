using System;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface IEmployeePasswordResetTokenRepository
    {
        Task AddAsync(EmployeePasswordResetToken token, CancellationToken ct = default);
        Task<EmployeePasswordResetToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task UpdateAsync(EmployeePasswordResetToken token, CancellationToken ct = default);
        Task InvalidateAllUserTokensAsync(Guid employeeId, CancellationToken ct = default);
    }
}
