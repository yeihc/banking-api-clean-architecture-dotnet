using System;
using BankingApi.Domain.Exceptions;
using BankingApi.Domain.Models;
using Xunit;

namespace BankingApi.Tests.Unit.Domain
{
    public class TransferTests
    {
        [Fact]
        public void Constructor_WithValidArguments_InitializesTransferInPendingStatus()
        {
            var id = Guid.NewGuid();
            var source = Guid.NewGuid();
            var target = Guid.NewGuid();
            var amount = Money.Of(150m);
            var idempotencyKey = "tx-12345";

            var transfer = new Transfer(id, source, target, amount, idempotencyKey);

            Assert.Equal(id, transfer.Id);
            Assert.Equal(source, transfer.SourceAccountId);
            Assert.Equal(target, transfer.TargetAccountId);
            Assert.Equal(amount, transfer.Amount);
            Assert.Equal(idempotencyKey, transfer.IdempotencyKey);
            Assert.Equal(TransferStatus.Pending, transfer.Status);
            Assert.True(transfer.CreatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void Constructor_WhenSourceAndTargetAreSame_ThrowsDomainException()
        {
            var sameId = Guid.NewGuid();
            var ex = Assert.Throws<DomainException>(() =>
                new Transfer(Guid.NewGuid(), sameId, sameId, Money.Of(100m), "key-1"));

            Assert.Equal("INVALID_TRANSFER", ex.Code);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Constructor_WhenAmountIsZeroOrNegative_ThrowsDomainException(decimal amount)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new Transfer(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Of(amount), "key-1"));

            Assert.Equal("INVALID_AMOUNT", ex.Code);
        }

        [Fact]
        public void Complete_WhenPending_TransitionsToCompleted()
        {
            var transfer = new Transfer(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Of(100m), "key-1");

            transfer.Complete();

            Assert.Equal(TransferStatus.Completed, transfer.Status);
        }

        [Fact]
        public void Complete_WhenAlreadyCompleted_ThrowsDomainException()
        {
            var transfer = new Transfer(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Of(100m), "key-1");
            transfer.Complete();

            var ex = Assert.Throws<DomainException>(() => transfer.Complete());
            Assert.Equal("INVALID_TRANSFER_STATE", ex.Code);
        }

        [Fact]
        public void Fail_WhenPending_TransitionsToFailed()
        {
            var transfer = new Transfer(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Of(100m), "key-1");

            transfer.Fail("Declined by system");

            Assert.Equal(TransferStatus.Failed, transfer.Status);
        }
    }
}
