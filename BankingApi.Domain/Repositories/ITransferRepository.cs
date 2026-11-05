using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApi.Domain.Models;

namespace BankingApi.Domain.Repositories
{
    /// <summary>
    /// Repository contract for the financial ledger.
    ///
    /// DESIGN DECISIONS:
    /// - Append-only behavior (no update or delete).
    /// - Optimized for audit and history reconstruction.
    /// </summary>
    public interface ITransactionRepository
    {
        Task SaveAsync(Transaction transaction);

        Task<IReadOnlyList<Transaction>> FindByAccountIdAsync(Guid accountId);
    }
}