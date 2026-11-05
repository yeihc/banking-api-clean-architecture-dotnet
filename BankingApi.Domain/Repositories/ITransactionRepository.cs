using System;
using System.Threading.Tasks;
using BankingApi.Domain.Models;

namespace BankingApi.Domain.Repositories
{
    /// <summary>
    /// Repository contract for Transfer persistence and idempotency control.
    ///
    /// DESIGN DECISIONS:
    /// - Explicit support for idempotency.
    /// - Transfers are treated as immutable records.
    /// </summary>
    public interface ITransferRepository
    {
        Task<Transfer?> FindByIdAsync(Guid transferId);

        Task<Transfer?> FindByIdempotencyKeyAsync(string idempotencyKey);

        Task SaveAsync(Transfer transfer);
    }
}