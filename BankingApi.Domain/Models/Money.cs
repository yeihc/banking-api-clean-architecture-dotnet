using System;

namespace BankingApi.Domain.Models
{
    /// <summary>
    /// Value Object representing a monetary amount.
    ///
    /// DESIGN DECISIONS:
    /// 1. Immutability: Money is immutable once created.
    /// 2. Precision: Uses decimal to avoid floating-point errors.
    /// 3. Validation: Prevents invalid monetary states (null, incorrect scale).
    /// 4. Value Semantics: Equality is based on value, not reference.
    /// </summary>
    public readonly struct Money : IEquatable<Money>, IComparable<Money>
    {
        private const int Scale = 2;

        public decimal Value { get; }

        private Money(decimal value)
        {
            Value = Math.Round(value, Scale, MidpointRounding.ToEven);
        }

        /// <summary>
        /// Factory method to create a Money instance.
        /// </summary>
        public static Money Of(decimal value)
        {
            return new Money(value);
        }

        /// <summary>
        /// Returns a Money instance with value zero.
        /// </summary>
        public static Money Zero() => new Money(0m);

        public bool IsZero() => Value == 0m;

        public bool IsNegative() => Value < 0m;

        public bool IsPositive() => Value > 0m;

        public Money Add(Money other)
        {
            return new Money(Value + other.Value);
        }

        public Money Subtract(Money other)
        {
            return new Money(Value - other.Value);
        }

        // -----------------------------
        // Equality & Comparison
        // -----------------------------

        public bool Equals(Money other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            return obj is Money other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public int CompareTo(Money other)
        {
            return Value.CompareTo(other.Value);
        }

        public static bool operator ==(Money left, Money right)
            => left.Equals(right);

        public static bool operator !=(Money left, Money right)
            => !left.Equals(right);

        public static bool operator >(Money left, Money right)
            => left.Value > right.Value;

        public static bool operator <(Money left, Money right)
            => left.Value < right.Value;

        public static bool operator >=(Money left, Money right)
            => left.Value >= right.Value;

        public static bool operator <=(Money left, Money right)
            => left.Value <= right.Value;

        public override string ToString()
        {
            return Value.ToString($"F{Scale}");
        }
    }
}
