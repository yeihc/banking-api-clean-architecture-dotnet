using System;
using BankingApi.Domain.Exceptions;

namespace BankingApi.Domain.Models
{
    /// <summary>
    /// Represents a funds transfer between two accounts.
    ///
    /// DESIGN DECISIONS:
    /// - Explicit lifecycle: Pending -> Completed / Failed
    /// - Idempotency enforced via IdempotencyKey
    /// - Immutable intent: account ids and amount cannot change
    /// - State changes are explicit (Complete / Fail)
    /// </summary>
    public class Transfer
    {
        public Guid Id { get; private set; }
        public Guid SourceAccountId { get; private set; }
        public Guid TargetAccountId { get; private set; }
        public Money Amount { get; private set; }
        public string IdempotencyKey { get; private set; }
        public TransferStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Required for ORMs / serializers
        #pragma warning disable CS8618
        private Transfer() { }
        #pragma warning restore CS8618

        public Transfer(
            Guid id,
            Guid sourceAccountId,
            Guid targetAccountId,
            Money amount,
            string idempotencyKey
        )
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Transfer id is required.", nameof(id));

            if (sourceAccountId == Guid.Empty)
                throw new ArgumentException("Source account id is required.", nameof(sourceAccountId));

            if (targetAccountId == Guid.Empty)
                throw new ArgumentException("Target account id is required.", nameof(targetAccountId));

            if (sourceAccountId == targetAccountId)
                throw new DomainException(
                    "INVALID_TRANSFER",
                    "Source and target accounts must be different."
                );

            if (amount.IsZero() || amount.IsNegative())
                throw new DomainException(
                    "INVALID_AMOUNT",
                    "Transfer amount must be greater than zero."
                );

            if (string.IsNullOrWhiteSpace(idempotencyKey))
                throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));

            Id = id;
            SourceAccountId = sourceAccountId;
            TargetAccountId = targetAccountId;
            Amount = amount;
            IdempotencyKey = idempotencyKey;
            Status = TransferStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        // -----------------------------
        // Lifecycle transitions
        // -----------------------------

        public void Complete()
        {
            EnsurePending();

            Status = TransferStatus.Completed;
        }

        public void Fail(string reason)
        {
            EnsurePending();

            Status = TransferStatus.Failed;
        }

        private void EnsurePending()
        {
            if (Status != TransferStatus.Pending)
                throw new DomainException(
                    "INVALID_TRANSFER_STATE",
                    $"Transfer cannot change status from {Status}."
                );
        }
    }

    public enum TransferStatus
    {
        Pending,
        Completed,
        Failed
    }
}