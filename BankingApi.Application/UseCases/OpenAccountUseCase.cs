using System;
using System.Threading.Tasks;
using BankingApi.Application.Commands;
using BankingApi.Domain.Models;
using BankingApi.Domain.Repositories;

namespace BankingApi.Application.UseCases
{
    /// <summary>
    /// Orchestrates the account opening use case.
    ///
    /// RESPONSIBILITIES:
    /// - Instantiate a new Account aggregate root.
    /// - Persist the account details.
    /// - Generate a ledger credit transaction if there is an initial deposit.
    /// </summary>
    public sealed class OpenAccountUseCase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;

        public OpenAccountUseCase(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository
        )
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<Account> ExecuteAsync(OpenAccountCommand command)
        {
            var account = new Account(Guid.NewGuid(), command.CustomerId, command.InitialDeposit);

            await _accountRepository.SaveAsync(account);

            if (command.InitialDeposit > Money.Zero())
            {
                var creditTx = Transaction.Credit(
                    account.Id,
                    command.InitialDeposit,
                    "Initial deposit"
                );
                await _transactionRepository.SaveAsync(creditTx);
            }

            return account;
        }
    }
}
