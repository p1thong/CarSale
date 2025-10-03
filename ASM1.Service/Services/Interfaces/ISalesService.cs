using ASM1.Repository.Models;

namespace ASM1.Service.Services.Interfaces
{
    public interface ISalesService
    {
        // Customer Management
        Task<Customer> CreateOrUpdateCustomerAsync(Customer customer);
        Task<Customer?> GetCustomerAsync(int customerId);
        Task<IEnumerable<Customer>> GetCustomersByDealerAsync(int dealerId);

        // Order Management
        Task<Order> CreateOrderAsync(Order order);
        Task<Order?> GetOrderAsync(int orderId);
        Task<Order> UpdateOrderStatusAsync(int orderId, string status);
        Task<IEnumerable<Order>> GetOrdersByDealerAsync(int dealerId);
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
        Task<IEnumerable<Order>> GetPendingOrdersByDealerAsync(int dealerId);
        Task<Order> ConfirmOrderAsync(int orderId, string dealerNotes = "");
        Task<Order> RejectOrderAsync(int orderId, string rejectionReason);

        // Sales Contract Management
        Task<SalesContract> CreateSalesContractAsync(int orderId, decimal totalAmount, string terms);
        Task<SalesContract?> GetSalesContractByOrderAsync(int orderId);

        // Payment Management
        Task<Payment> ProcessPaymentAsync(int orderId, decimal amount, string paymentMethod);
        Task<IEnumerable<Payment>> GetPaymentsByOrderAsync(int orderId);
        Task<decimal> GetRemainingBalanceAsync(int orderId);

        // Vehicle Delivery
        Task<Order> CompleteOrderAsync(int orderId);
        Task<bool> IsOrderReadyForDeliveryAsync(int orderId);
    }
}