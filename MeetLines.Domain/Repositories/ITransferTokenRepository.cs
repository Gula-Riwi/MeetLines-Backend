using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Repositories
{
    public interface ITransferTokenRepository
    {
        Task AddAsync(TransferToken token, CancellationToken ct = default);
        Task<TransferToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task UpdateAsync(TransferToken token, CancellationToken ct = default);
        Task DeleteAsync(System.Guid id, CancellationToken ct = default);
    }
}
