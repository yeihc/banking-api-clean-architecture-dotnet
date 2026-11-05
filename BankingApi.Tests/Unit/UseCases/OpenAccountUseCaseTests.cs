using System;
using System.Threading.Tasks;
using BankingApi.Application.Commands;
using BankingApi.Application.UseCases;
using BankingApi.Domain.Models;
using BankingApi.Domain.Repositories;
using Moq;
using Xunit;

namespace BankingApi.Tests.Unit.UseCases
{
    public class OpenAccountUseCaseTests
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly OpenAccountUseCase _useCase;

        public OpenAccountUseCaseTests()
        {
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _useCase = new OpenAccountUseCase(
                _accountRepositoryMock.Object,
                _transactionRepositoryMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_WithPositiveInitialDeposit_CreatesAccountAndCreditTransaction()
        {
            var customerId = Guid.NewGuid();
            var initialDeposit = Money.Of(250.00m);
            var command = new OpenAccountCommand(customerId, initialDeposit);

            var account = await _useCase.ExecuteAsync(command);

            Assert.NotNull(account);
            Assert.Equal(customerId, account.CustomerId);
            Assert.Equal(initialDeposit, account.Balance);
            Assert.Equal(AccountStatus.Active, account.Status);

            _accountRepositoryMock.Verify(r => r.SaveAsync(It.Is<Account>(a =>
                a.Id == account.Id &&
                a.CustomerId == customerId &&
                a.Balance == initialDeposit)), Times.Once);

            _transactionRepositoryMock.Verify(r => r.SaveAsync(It.Is<Transaction>(t =>
                t.AccountId == account.Id &&
                t.Type == TransactionType.Credit &&
                t.Amount == initialDeposit &&
                t.Description == "Initial deposit")), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithZeroInitialDeposit_CreatesAccountWithoutCreditTransaction()
        {
            var customerId = Guid.NewGuid();
            var command = new OpenAccountCommand(customerId, Money.Zero());

            var account = await _useCase.ExecuteAsync(command);

            Assert.NotNull(account);
            Assert.Equal(customerId, account.CustomerId);
            Assert.True(account.Balance.IsZero());

            _accountRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Account>()), Times.Once);
            _transactionRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Transaction>()), Times.Never);
        }
    }
}
