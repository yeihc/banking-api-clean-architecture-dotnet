using System;
using System.Threading.Tasks;
using BankingApi.Application.Commands;
using BankingApi.Application.UseCases;
using BankingApi.Domain.Exceptions;
using BankingApi.Domain.Models;
using BankingApi.Domain.Repositories;
using Moq;
using Xunit;

namespace BankingApi.Tests.Unit.UseCases
{
    public class TransferFundsUseCaseTests
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<ITransferRepository> _transferRepositoryMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly TransferFundsUseCase _useCase;

        public TransferFundsUseCaseTests()
        {
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _transferRepositoryMock = new Mock<ITransferRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();

            _useCase = new TransferFundsUseCase(
                _accountRepositoryMock.Object,
                _transferRepositoryMock.Object,
                _transactionRepositoryMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_WhenTransferAlreadyExists_IsIdempotentAndDoesNotExecuteAgain()
        {
            var key = "existing-key-123";
            var existingTransfer = new Transfer(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Of(50m), key);

            _transferRepositoryMock
                .Setup(r => r.FindByIdempotencyKeyAsync(key))
                .ReturnsAsync(existingTransfer);

            var command = new TransferFundsCommand(
                existingTransfer.SourceAccountId,
                existingTransfer.TargetAccountId,
                Money.Of(50m),
                key
            );

            await _useCase.ExecuteAsync(command);

            _accountRepositoryMock.Verify(r => r.FindByIdAsync(It.IsAny<Guid>()), Times.Never);
            _accountRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenSourceAccountNotFound_ThrowsDomainException()
        {
            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            _transferRepositoryMock
                .Setup(r => r.FindByIdempotencyKeyAsync(It.IsAny<string>()))
                .ReturnsAsync((Transfer?)null);

            _accountRepositoryMock
                .Setup(r => r.FindByIdAsync(sourceId))
                .ReturnsAsync((Account?)null);

            var command = new TransferFundsCommand(sourceId, targetId, Money.Of(50m), "key-1");

            var ex = await Assert.ThrowsAsync<DomainException>(() => _useCase.ExecuteAsync(command));
            Assert.Equal("ACCOUNT_NOT_FOUND", ex.Code);
        }

        [Fact]
        public async Task ExecuteAsync_HappyPath_DebitsSourceCreditsTargetAndCreatesLedgerRecords()
        {
            var source = new Account(Guid.NewGuid(), Guid.NewGuid(), Money.Of(200m));
            var target = new Account(Guid.NewGuid(), Guid.NewGuid(), Money.Of(50m));
            var transferAmount = Money.Of(75m);
            var key = "new-transfer-key";

            _transferRepositoryMock
                .Setup(r => r.FindByIdempotencyKeyAsync(key))
                .ReturnsAsync((Transfer?)null);

            _accountRepositoryMock
                .Setup(r => r.FindByIdAsync(source.Id))
                .ReturnsAsync(source);

            _accountRepositoryMock
                .Setup(r => r.FindByIdAsync(target.Id))
                .ReturnsAsync(target);

            var command = new TransferFundsCommand(source.Id, target.Id, transferAmount, key);

            await _useCase.ExecuteAsync(command);

            Assert.Equal(Money.Of(125m), source.Balance);
            Assert.Equal(Money.Of(125m), target.Balance);

            _transferRepositoryMock.Verify(r => r.SaveAsync(It.Is<Transfer>(t =>
                t.SourceAccountId == source.Id &&
                t.TargetAccountId == target.Id &&
                t.Amount == transferAmount &&
                t.IdempotencyKey == key)), Times.AtLeastOnce);

            _accountRepositoryMock.Verify(r => r.SaveAsync(source), Times.Once);
            _accountRepositoryMock.Verify(r => r.SaveAsync(target), Times.Once);

            _transactionRepositoryMock.Verify(r => r.SaveAsync(It.Is<Transaction>(tx =>
                tx.AccountId == source.Id &&
                tx.Type == TransactionType.Debit &&
                tx.Amount == transferAmount)), Times.Once);

            _transactionRepositoryMock.Verify(r => r.SaveAsync(It.Is<Transaction>(tx =>
                tx.AccountId == target.Id &&
                tx.Type == TransactionType.Credit &&
                tx.Amount == transferAmount)), Times.Once);
        }
    }
}
