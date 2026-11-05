using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankingApi.Domain.Models;
using BankingApi.Domain.Repositories;
using BankingApi.Infrastructure.Persistence;

namespace BankingApi.Infrastructure.Repositories
{
    public sealed class EfTransactionRepository : ITransactionRepository
    {
        private readonly BankingDbContext _context;

        public EfTransactionRepository(BankingDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Transaction>> FindByAccountIdAsync(Guid accountId)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}