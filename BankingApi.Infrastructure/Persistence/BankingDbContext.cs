using Microsoft.EntityFrameworkCore;
using BankingApi.Domain.Models;

namespace BankingApi.Infrastructure.Persistence
{
    public class BankingDbContext : DbContext
    {
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transfer> Transfers => Set<Transfer>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        public BankingDbContext(DbContextOptions<BankingDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankingDbContext).Assembly);
        }
    }
}