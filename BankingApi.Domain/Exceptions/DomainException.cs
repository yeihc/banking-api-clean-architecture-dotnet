using System;

namespace BankingApi.Domain.Exceptions
{
    /// <summary>
    /// Represents a business rule violation within the Domain.
    ///
    /// DESIGN DECISIONS:
    /// 1. Explicit Error Codes: Allows the application and API layers
    ///    to distinguish different business errors programmatically.
    /// 2. Human-readable Messages: Safe to expose to clients if needed.
    /// 3. Domain-centric: This exception is thrown ONLY from domain logic.
    /// </summary>
    public sealed class DomainException : Exception
    {
        /// <summary>
        /// Machine-readable business error code.
        /// Example: "INSUFFICIENT_FUNDS", "INVALID_STATUS_CHANGE"
        /// </summary>
        public string Code { get; }

        public DomainException(string code, string message)
            : base(message)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Domain error code is required.", nameof(code));

            Code = code;
        }
    }
}