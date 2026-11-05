using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankingApi.Domain.Models;

namespace BankingApi.Infrastructure.Persistence.Configurations
{
    public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("accounts");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.AccountNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(a => a.AccountNumber)
                .IsUnique();

            builder.Property(a => a.Balance)
                .HasConversion(
                    money => money.Value,
                    value => Money.Of(value)
                )
                .IsRequired();

            builder.Property(a => a.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(a => a.Version)
                .IsConcurrencyToken();
        }
    }
}