using ASM1.Repository.Data;
using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASM1.Repository.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CarSalesDbContext _context;

        public CustomerRepository(CarSalesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.Include(c => c.Dealer).ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            return await _context
                .Customers.Include(c => c.Dealer)
                .Include(c => c.Orders)
                .Include(c => c.Quotations)
                .Include(c => c.TestDrives)
                .Include(c => c.Feedbacks)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _context
                .Customers.Include(c => c.Dealer)
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByDealerAsync(int dealerId)
        {
            return await _context
                .Customers.Where(c => c.DealerId == dealerId)
                .Include(c => c.Dealer)
                .ToListAsync();
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            // Get the next available CustomerId
            var maxCustomerId = await _context.Customers.MaxAsync(c => (int?)c.CustomerId) ?? 0;
            customer.CustomerId = maxCustomerId + 1;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
                return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            return await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
        }
    }
}
