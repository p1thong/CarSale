using ASM1.Repository.Data;
using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASM1.Repository.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly CarSalesDbContext _context;

        public OrderRepository(CarSalesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context
                .Orders.Include(o => o.Customer)
                .Include(o => o.Dealer)
                .Include(o => o.Variant)
                .ThenInclude(v => v.VehicleModel)
                .Include(o => o.Payments)
                .Include(o => o.SalesContracts)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context
                .Orders.Include(o => o.Customer)
                .Include(o => o.Dealer)
                .Include(o => o.Variant)
                .ThenInclude(v => v.VehicleModel)
                .Include(o => o.Payments)
                .Include(o => o.SalesContracts)
                .Include(o => o.Promotions)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            return await _context
                .Orders.Where(o => o.CustomerId == customerId)
                .Include(o => o.Dealer)
                .Include(o => o.Variant)
                .ThenInclude(v => v.VehicleModel)
                .Include(o => o.Payments)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByDealerAsync(int dealerId)
        {
            return await _context
                .Orders.Where(o => o.DealerId == dealerId)
                .Include(o => o.Customer)
                .Include(o => o.Variant)
                .ThenInclude(v => v.VehicleModel)
                .Include(o => o.Payments)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _context
                .Orders.Where(o => o.Status == status)
                .Include(o => o.Customer)
                .Include(o => o.Dealer)
                .Include(o => o.Variant)
                .ThenInclude(v => v.VehicleModel)
                .ToListAsync();
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            // Get the next available OrderId
            var maxOrderId = await _context.Orders.MaxAsync(o => (int?)o.OrderId) ?? 0;
            order.OrderId = maxOrderId + 1;
            
            order.OrderDate = DateOnly.FromDateTime(DateTime.Now);
            order.Status = "Pending";
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await GetOrderByIdAsync(orderId);
        }
    }
}
