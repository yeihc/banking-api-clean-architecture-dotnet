using System;
using BankingApi.Domain.Models;

namespace BankingApi.Application.Commands
{
    /// <summary>
    /// Command representing the intent to transfer funds between accounts.
    ///
    /// DESIGN DECISIONS:
    /// - Immutable
    /// - Validated at creation time
    /// - Carries idempotency information
    /// </summary>
    public sealed class TransferFundsCommand
    {
        public Guid SourceAccountId { get; }
        public Guid TargetAccountId { get; }
        public Money Amount { get; }
        public string IdempotencyKey { get; }

        public TransferFundsCommand(
            Guid sourceAccountId,
            Guid targetAccountId,
            Money amount,
            string idempotencyKey
        )
        {
            if (sourceAccountId == Guid.Empty)
                throw new ArgumentException("Source account id is required.", nameof(sourceAccountId));

            if (targetAccountId == Guid.Empty)
                throw new ArgumentException("Target account id is required.", nameof(targetAccountId));

            if (amount.IsZero() || amount.IsNegative())
                throw new ArgumentException("Transfer amount must be greater than zero.", nameof(amount));

            if (string.IsNullOrWhiteSpace(idempotencyKey))
                throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));

            SourceAccountId = sourceAccountId;
            TargetAccountId = targetAccountId;
            Amount = amount;
            IdempotencyKey = idempotencyKey;
        }
    }
}
