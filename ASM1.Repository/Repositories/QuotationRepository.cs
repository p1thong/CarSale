using ASM1.Repository.Data;
using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASM1.Repository.Repositories
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly CarSalesDbContext _context;

        public QuotationRepository(CarSalesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Quotation>> GetAllQuotationsAsync()
        {
            return await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.Dealer)
                .Include(q => q.Variant)
                    .ThenInclude(v => v.VehicleModel)
                .ToListAsync();
        }

        public async Task<Quotation?> GetQuotationByIdAsync(int quotationId)
        {
            return await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.Dealer)
                .Include(q => q.Variant)
                    .ThenInclude(v => v.VehicleModel)
                .FirstOrDefaultAsync(q => q.QuotationId == quotationId);
        }

        public async Task<IEnumerable<Quotation>> GetQuotationsByCustomerAsync(int customerId)
        {
            return await _context.Quotations
                .Where(q => q.CustomerId == customerId)
                .Include(q => q.Dealer)
                .Include(q => q.Variant)
                    .ThenInclude(v => v.VehicleModel)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quotation>> GetQuotationsByDealerAsync(int dealerId)
        {
            return await _context.Quotations
                .Where(q => q.DealerId == dealerId)
                .Include(q => q.Customer)
                .Include(q => q.Variant)
                    .ThenInclude(v => v.VehicleModel)
                .ToListAsync();
        }

        public async Task<Quotation> CreateQuotationAsync(Quotation quotation)
        {
            quotation.CreatedAt = DateTime.Now;
            quotation.Status = "Pending";
            _context.Quotations.Add(quotation);
            await _context.SaveChangesAsync();
            return quotation;
        }

        public async Task<Quotation> UpdateQuotationAsync(Quotation quotation)
        {
            _context.Entry(quotation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return quotation;
        }

        public async Task<bool> DeleteQuotationAsync(int quotationId)
        {
            var quotation = await _context.Quotations.FindAsync(quotationId);
            if (quotation == null) return false;

            _context.Quotations.Remove(quotation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Quotation>> GetQuotationsByStatusAsync(string status)
        {
            return await _context.Quotations
                .Where(q => q.Status == status)
                .Include(q => q.Customer)
                .Include(q => q.Dealer)
                .Include(q => q.Variant)
                    .ThenInclude(v => v.VehicleModel)
                .ToListAsync();
        }
    }
}