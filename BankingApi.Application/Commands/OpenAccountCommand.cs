using System;
using BankingApi.Domain.Models;

namespace BankingApi.Application.Commands
{
    /// <summary>
    /// Command representing the intent to open a new account for a customer.
    ///
    /// DESIGN DECISIONS:
    /// - Immutable data carrier (Command)
    /// - Fail-fast validation (prevents invalid requests reaching the use case)
    /// </summary>
    public sealed class OpenAccountCommand
    {
        public Guid CustomerId { get; }
        public Money InitialDeposit { get; }

        public OpenAccountCommand(Guid customerId, Money initialDeposit)
        {
            if (customerId == Guid.Empty)
                throw new ArgumentException("Customer id is required.", nameof(customerId));

            // Business rule: opening with negative balance is not allowed.
            if (initialDeposit.IsNegative())
                throw new ArgumentException("Initial deposit cannot be negative.", nameof(initialDeposit));

            CustomerId = customerId;
            InitialDeposit = initialDeposit;
        }
    }
}