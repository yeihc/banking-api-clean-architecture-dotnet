using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankingApi.Domain.Models;
using BankingApi.Domain.Repositories;
using BankingApi.Infrastructure.Persistence;

namespace BankingApi.Infrastructure.Repositories
{
    public sealed class EfTransferRepository : ITransferRepository
    {
        private readonly BankingDbContext _context;

        public EfTransferRepository(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Transfer?> FindByIdAsync(Guid transferId)
        {
            return await _context.Transfers.FindAsync(transferId);
        }

        public async Task<Transfer?> FindByIdempotencyKeyAsync(string idempotencyKey)
        {
            return await _context.Transfers
                .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey);
        }

        public async Task SaveAsync(Transfer transfer)
        {
            var entry = _context.Entry(transfer);
            if (entry.State == EntityState.Detached)
            {
                var exists = await _context.Transfers.AnyAsync(t => t.Id == transfer.Id);
                if (!exists)
                {
                    _context.Transfers.Add(transfer);
                }
                else
                {
                    _context.Transfers.Update(transfer);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
