using System;
using System.Threading.Tasks;
using BankingApi.Domain.Models;

namespace BankingApi.Domain.Repositories
{
    /// <summary>
    /// Repository contract for the Account aggregate root.
    ///
    /// DESIGN DECISIONS:
    /// - Domain defines the contract, not the implementation.
    /// - Only aggregate roots are exposed.
    /// - Async by default for IO-bound operations.
    /// </summary>
    public interface IAccountRepository
    {
        Task<Account?> FindByIdAsync(Guid accountId);

        Task SaveAsync(Account account);
    }
}