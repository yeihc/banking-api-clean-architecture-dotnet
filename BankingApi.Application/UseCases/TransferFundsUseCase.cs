using System;
using System.Threading.Tasks;
using BankingApi.Application.Commands;
using BankingApi.Domain.Exceptions;
using BankingApi.Domain.Models;
using BankingApi.Domain.Repositories;

namespace BankingApi.Application.UseCases
{
    /// <summary>
    /// Orchestrates the fund transfer use case.
    ///
    /// RESPONSIBILITIES:
    /// - Enforce idempotency
    /// - Load and modify aggregates
    /// - Persist state changes
    /// - Create ledger entries
    /// </summary>
    public sealed class TransferFundsUseCase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransferRepository _transferRepository;
        private readonly ITransactionRepository _transactionRepository;

        public TransferFundsUseCase(
            IAccountRepository accountRepository,
            ITransferRepository transferRepository,
            ITransactionRepository transactionRepository
        )
        {
            _accountRepository = accountRepository;
            _transferRepository = transferRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task ExecuteAsync(TransferFundsCommand command)
        {
            // 1. Idempotency check
            var existingTransfer =
                await _transferRepository.FindByIdempotencyKeyAsync(command.IdempotencyKey);

            if (existingTransfer != null)
            {
                // Transfer already processed (idempotent)
                return;
            }

            // 2. Load accounts
            var sourceAccount =
                await _accountRepository.FindByIdAsync(command.SourceAccountId)
                ?? throw new DomainException(
                    "ACCOUNT_NOT_FOUND",
                    "Source account not found."
                );

            var targetAccount =
                await _accountRepository.FindByIdAsync(command.TargetAccountId)
                ?? throw new DomainException(
                    "ACCOUNT_NOT_FOUND",
                    "Target account not found."
                );

            // 3. Create transfer record (PENDING)
            var transfer = new Transfer(
                Guid.NewGuid(),
                sourceAccount.Id,
                targetAccount.Id,
                command.Amount,
                command.IdempotencyKey
            );

            await _transferRepository.SaveAsync(transfer);

            // 4. Apply business operations
            sourceAccount.Debit(command.Amount);
            targetAccount.Credit(command.Amount);

            // 5. Persist accounts
            await _accountRepository.SaveAsync(sourceAccount);
            await _accountRepository.SaveAsync(targetAccount);

            // 6. Ledger entries
            var debitTx = Transaction.Debit(
                sourceAccount.Id,
                command.Amount,
                $"Transfer to account {targetAccount.AccountNumber}"
            );

            var creditTx = Transaction.Credit(
                targetAccount.Id,
                command.Amount,
                $"Transfer from account {sourceAccount.AccountNumber}"
            );

            await _transactionRepository.SaveAsync(debitTx);
            await _transactionRepository.SaveAsync(creditTx);

            // 7. Mark transfer as completed
            transfer.Complete();
            await _transferRepository.SaveAsync(transfer);
        }
    }
}