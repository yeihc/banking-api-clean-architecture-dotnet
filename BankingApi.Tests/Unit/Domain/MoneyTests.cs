using System;
using System.Globalization;
using BankingApi.Domain.Models;
using Xunit;

namespace BankingApi.Tests.Unit.Domain
{
    public class MoneyTests
    {
        [Fact]
        public void Of_RoundsToTwoDecimalPlaces()
        {
            var money = Money.Of(123.456m);
            Assert.Equal(123.46m, money.Value);
        }

        [Fact]
        public void Zero_ReturnsMoneyWithZeroValue()
        {
            var zero = Money.Zero();
            Assert.Equal(0.00m, zero.Value);
            Assert.True(zero.IsZero());
        }

        [Fact]
        public void ValueChecks_IdentifyPositiveAndNegative()
        {
            var positive = Money.Of(10.50m);
            var negative = Money.Of(-5.00m);

            Assert.True(positive.IsPositive());
            Assert.False(positive.IsNegative());

            Assert.True(negative.IsNegative());
            Assert.False(negative.IsPositive());
        }

        [Fact]
        public void Add_ReturnsSumOfBothAmounts()
        {
            var m1 = Money.Of(50.25m);
            var m2 = Money.Of(25.75m);

            var result = m1.Add(m2);

            Assert.Equal(76.00m, result.Value);
        }

        [Fact]
        public void Subtract_ReturnsDifferenceOfBothAmounts()
        {
            var m1 = Money.Of(100.00m);
            var m2 = Money.Of(40.50m);

            var result = m1.Subtract(m2);

            Assert.Equal(59.50m, result.Value);
        }

        [Fact]
        public void EqualityAndComparisons_WorkCorrectly()
        {
            var a = Money.Of(100m);
            var b = Money.Of(100m);
            var c = Money.Of(50m);

            Assert.True(a == b);
            Assert.False(a != b);
            Assert.True(a > c);
            Assert.True(c < a);
            Assert.True(a >= b);
            Assert.True(c <= a);
            Assert.True(a.Equals((object)b));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ToString_FormatsWithTwoDecimals()
        {
            var money = Money.Of(42m);
            var str = money.ToString();
            Assert.True(str == "42.00" || str == "42,00");
        }
    }
}
