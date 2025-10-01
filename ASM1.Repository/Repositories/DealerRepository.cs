using ASM1.Repository.Data;
using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASM1.Repository.Repositories
{
    public class DealerRepository : IDealerRepository
    {
        private readonly CarSalesDbContext _context;

        public DealerRepository(CarSalesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Dealer>> GetAllDealersAsync()
        {
            return await _context.Dealers
                .Include(d => d.DealerContracts)
                .Include(d => d.Customers)
                .Include(d => d.Orders)
                .ToListAsync();
        }

        public async Task<Dealer?> GetDealerByIdAsync(int dealerId)
        {
            return await _context.Dealers
                .Include(d => d.DealerContracts)
                .Include(d => d.Customers)
                .Include(d => d.Orders)
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.DealerId == dealerId);
        }

        public async Task<Dealer?> GetDealerByEmailAsync(string email)
        {
            return await _context.Dealers
                .FirstOrDefaultAsync(d => d.Email == email);
        }

        public async Task<Dealer> CreateDealerAsync(Dealer dealer)
        {
            _context.Dealers.Add(dealer);
            await _context.SaveChangesAsync();
            return dealer;
        }

        public async Task<Dealer> UpdateDealerAsync(Dealer dealer)
        {
            _context.Entry(dealer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return dealer;
        }

        public async Task<bool> DeleteDealerAsync(int dealerId)
        {
            var dealer = await _context.Dealers.FindAsync(dealerId);
            if (dealer == null) return false;

            _context.Dealers.Remove(dealer);
            await _context.SaveChangesAsync();
            return true;
        }

        // Dealer Contract Management
        public async Task<IEnumerable<DealerContract>> GetDealerContractsAsync(int dealerId)
        {
            return await _context.DealerContracts
                .Where(dc => dc.DealerId == dealerId)
                .Include(dc => dc.Dealer)
                .ToListAsync();
        }

        public async Task<DealerContract> CreateDealerContractAsync(DealerContract contract)
        {
            _context.DealerContracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<DealerContract> UpdateDealerContractAsync(DealerContract contract)
        {
            _context.Entry(contract).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return contract;
        }
    }
}