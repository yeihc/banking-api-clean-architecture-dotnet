using System;
using BankingApi.Domain.Exceptions;

namespace BankingApi.Domain.Models
{
    /// <summary>
    /// Aggregate Root representing a Bank Account.
    ///
    /// RESPONSIBILITIES:
    /// - Protect balance invariants
    /// - Control lifecycle (Active -> Closed)
    /// - Generate ledger movements (via domain logic)
    ///
    /// DESIGN DECISIONS:
    /// - Aggregate Root: All balance modifications go through this class.
    /// - Invariants enforced internally (no negative balance, no operations on closed accounts).
    /// - No persistence concerns (pure domain model).
    /// </summary>
    public class Account
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public string AccountNumber { get; private set; }
        public Money Balance { get; private set; }
        public AccountStatus Status { get; private set; }

        /// <summary>
        /// Version used for optimistic concurrency control.
        /// Infrastructure layer will map this.
        /// </summary>
        public int Version { get; private set; }

        // Required for ORMs / serializers (kept internal to avoid misuse)
        #pragma warning disable CS8618
        private Account() { }
        #pragma warning restore CS8618

        public Account(Guid id, Guid customerId, Money initialBalance)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Account id is required.", nameof(id));

            if (customerId == Guid.Empty)
                throw new ArgumentException("Customer id is required.", nameof(customerId));

            Id = id;
            CustomerId = customerId;
            Balance = initialBalance;
            Status = AccountStatus.Active;
            AccountNumber = GenerateAccountNumber();
            Version = 0;
        }

        // -----------------------------
        // Business operations
        // -----------------------------

        public void Credit(Money amount)
        {
            EnsureAccountIsActive();

            if (amount.IsNegative() || amount.IsZero())
                throw new DomainException(
                    "INVALID_AMOUNT",
                    "Credit amount must be greater than zero."
                );

            Balance = Balance.Add(amount);
            Version++;
        }

        public void Debit(Money amount)
        {
            EnsureAccountIsActive();

            if (amount.IsNegative() || amount.IsZero())
                throw new DomainException(
                    "INVALID_AMOUNT",
                    "Debit amount must be greater than zero."
                );

            if (Balance < amount)
                throw new DomainException(
                    "INSUFFICIENT_FUNDS",
                    "Not enough balance to perform the operation."
                );

            Balance = Balance.Subtract(amount);
            Version++;
        }

        public void Close(string reason)
        {
            if (Status == AccountStatus.Closed)
            {
                // Idempotent close
                return;
            }

            if (!Balance.IsZero())
                throw new DomainException(
                    "INVALID_STATUS_CHANGE",
                    "Cannot close account with remaining balance."
                );

            Status = AccountStatus.Closed;
            Version++;
        }

        // -----------------------------
        // Invariant enforcement
        // -----------------------------

        private void EnsureAccountIsActive()
        {
            if (Status != AccountStatus.Active)
                throw new DomainException(
                    "ACCOUNT_CLOSED",
                    "Operations are not allowed on a closed account."
                );
        }

        private static string GenerateAccountNumber()
        {
            return $"ACC-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }
    }

    public enum AccountStatus
    {
        Active,
        Closed
    }
}
