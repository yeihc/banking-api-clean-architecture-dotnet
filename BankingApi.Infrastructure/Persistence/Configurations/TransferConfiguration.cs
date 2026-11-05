using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankingApi.Domain.Models;

namespace BankingApi.Infrastructure.Persistence.Configurations
{
    public sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
    {
        public void Configure(EntityTypeBuilder<Transfer> builder)
        {
            builder.ToTable("transfers");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.SourceAccountId)
                .IsRequired();

            builder.Property(t => t.TargetAccountId)
                .IsRequired();

            builder.HasIndex(t => t.SourceAccountId);
            builder.HasIndex(t => t.TargetAccountId);

            builder.Property(t => t.Amount)
                .HasConversion(
                    money => money.Value,
                    value => Money.Of(value)
                )
                .IsRequired();

            builder.Property(t => t.IdempotencyKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(t => t.IdempotencyKey)
                .IsUnique();

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(t => t.CreatedAt)
                .IsRequired();
        }

    }
}