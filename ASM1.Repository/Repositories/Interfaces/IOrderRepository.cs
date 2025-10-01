using ASM1.Repository.Models;

namespace ASM1.Repository.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
        Task<IEnumerable<Order>> GetOrdersByDealerAsync(int dealerId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int orderId);
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
    }
}