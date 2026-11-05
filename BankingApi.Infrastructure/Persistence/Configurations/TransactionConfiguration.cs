using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankingApi.Domain.Models;

namespace BankingApi.Infrastructure.Persistence.Configurations
{
    public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("account_transactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.AccountId)
                .IsRequired();

            builder.HasIndex(t => t.AccountId);

            builder.Property(t => t.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(t => t.Amount)
                .HasConversion(
                    money => money.Value,
                    value => Money.Of(value)
                )
                .IsRequired();

            builder.Property(t => t.Description)
                .HasMaxLength(255);

            builder.Property(t => t.CreatedAt)
                .IsRequired();
        }
    }
}
