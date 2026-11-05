using System;
using BankingApi.Domain.Exceptions;
using BankingApi.Domain.Models;
using Xunit;

namespace BankingApi.Tests.Unit.Domain
{
    public class AccountTests
    {
        [Fact]
        public void Constructor_WithValidArguments_InitializesAccountCorrectly()
        {
            var id = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var initialDeposit = Money.Of(500m);

            var account = new Account(id, customerId, initialDeposit);

            Assert.Equal(id, account.Id);
            Assert.Equal(customerId, account.CustomerId);
            Assert.Equal(initialDeposit, account.Balance);
            Assert.Equal(AccountStatus.Active, account.Status);
            Assert.StartsWith("ACC-", account.AccountNumber);
            Assert.Equal(0, account.Version);
        }

        [Fact]
        public void Constructor_WithEmptyIdOrCustomerId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new Account(Guid.Empty, Guid.NewGuid(), Money.Of(100m)));

            Assert.Throws<ArgumentException>(() =>
                new Account(Guid.NewGuid(), Guid.Empty, Money.Of(100m)));
        }

        [Fact]
        public void Credit_WithValidAmount_IncreasesBalance()
        {
            var account = new Account(Guid.NewGuid(), Guid.NewGuid(), Money.Of(100m));

            account.Credit(Money.Of(50m));

            Assert.Equal(Money.Of(150m), account.Balance);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Credit_WithZeroOrNegativeAmount_ThrowsDomainException(decimal amount)
        {
            var account = new Account(Guid.NewGuid(), Guid.NewGuid(), Money.Of(100m));

            var ex = Assert.Throws<DomainException>(() => account.Credit(Money.Of(amount)));
            Assert.Equal("INVALID_AMOUNT", ex.Code);
        }

        [Fact]
        public void Debit_WithSufficientFunds_DecreasesBalance()
        {
            var account = new Account(Guid.NewGuid(), Guid.NewGuid(), Money.Of(200m));

            account.Debit(Money.Of(75m));

            Assert.Equal(Money.Of(125m), account.Balance);
        }

        [Fact]
        public void Debit_WithInsufficientFunds_ThrowsDomainException()
        {
            var account = new Account(Guid.NewGuid(), Guid.NewGuid(), Money.Of(50m));

            var ex = Assert.Throws<DomainException>(() => account.Debit(Money.Of(100m)));
            Assert.Equal("INSUFFICIENT_FUNDS", ex.Code);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Debit_WithZeroOrNegativeAmount_ThrowsDomainException(decimal amount)
        {
            var account = new Account(Guid.NewGuid(), Guid.NewGuid(), Money.Of(100m));

            var ex = Assert.Throws<DomainException>(() => account.Debit(Money.Of(amount)));
            Assert.Equal("INVALID_AMOUNT", ex.Code);
        }

        [Fact]
        public void Close_WithZeroBalance_TransitionsToClosed()
        {
            var account = new Account(Guid.NewGuid(), Guid.NewGuid(), Money.Zero());

            account.Close("Customer request");

            Assert.Equal(AccountStatus.Closed, account.Status);
        }

        [Fact]
        public void Close_WithRemainingBalance_ThrowsDomainException()
        {
            var account = new Account(Guid.NewGuid(), Guid.NewGuid(), Money.Of(50m));

            var ex = Assert.Throws<DomainException>(() => account.Close("Customer request"));
            Assert.Equal("INVALID_STATUS_CHANGE", ex.Code);
        }

        [Fact]
        public void Operations_OnClosedAccount_ThrowDomainException()
        {
            var account = new Account(Guid.NewGuid(), Guid.NewGuid(), Money.Zero());
            account.Close("Customer request");

            var exCredit = Assert.Throws<DomainException>(() => account.Credit(Money.Of(10m)));
            Assert.Equal("ACCOUNT_CLOSED", exCredit.Code);

            var exDebit = Assert.Throws<DomainException>(() => account.Debit(Money.Of(10m)));
            Assert.Equal("ACCOUNT_CLOSED", exDebit.Code);
        }
    }
}
