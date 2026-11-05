using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankingApi.Domain.Models;
using BankingApi.Domain.Repositories;
using BankingApi.Infrastructure.Persistence;

namespace BankingApi.Infrastructure.Repositories
{
    public sealed class EfAccountRepository : IAccountRepository
    {
        private readonly BankingDbContext _context;

        public EfAccountRepository(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> FindByIdAsync(Guid accountId)
        {
            return await _context.Accounts.FindAsync(accountId);
        }

        public async Task SaveAsync(Account account)
        {
            var entry = _context.Entry(account);
            if (entry.State == EntityState.Detached)
            {
                var exists = await _context.Accounts.AnyAsync(a => a.Id == account.Id);
                if (!exists)
                {
                    _context.Accounts.Add(account);
                }
                else
                {
                    _context.Accounts.Update(account);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
