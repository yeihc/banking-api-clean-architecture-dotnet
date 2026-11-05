using System;

namespace BankingApi.Domain.Models
{
    /// <summary>
    /// Represents an immutable ledger entry for an Account.
    ///
    /// DESIGN DECISIONS:
    /// - Append-only: Transactions are never updated or deleted.
    /// - Immutable: Once created, a transaction cannot change.
    /// - Explicit type: Debit or Credit.
    /// - Auditable: Captures when and why the movement occurred.
    /// </summary>
    public sealed class Transaction
    {
        public Guid Id { get; private set; }
        public Guid AccountId { get; private set; }
        public TransactionType Type { get; private set; }
        public Money Amount { get; private set; }
        public string Description { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Required for ORMs / serializers
        #pragma warning disable CS8618
        private Transaction() { }
        #pragma warning restore CS8618

        private Transaction(
            Guid id,
            Guid accountId,
            TransactionType type,
            Money amount,
            string description
        )
        {
            Id = id;
            AccountId = accountId;
            Type = type;
            Amount = amount;
            Description = description;
            CreatedAt = DateTime.UtcNow;
        }

        // -----------------------------
        // Factory methods
        // -----------------------------

        public static Transaction Debit(
            Guid accountId,
            Money amount,
            string description
        )
        {
            return new Transaction(
                Guid.NewGuid(),
                accountId,
                TransactionType.Debit,
                amount,
                description
            );
        }

        public static Transaction Credit(
            Guid accountId,
            Money amount,
            string description
        )
        {
            return new Transaction(
                Guid.NewGuid(),
                accountId,
                TransactionType.Credit,
                amount,
                description
            );
        }
    }

    public enum TransactionType
    {
        Debit,
        Credit
    }
}