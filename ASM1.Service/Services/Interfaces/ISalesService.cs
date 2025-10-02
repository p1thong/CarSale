using ASM1.Repository.Models;

namespace ASM1.Service.Services.Interfaces
{
    public interface ISalesService
    {
        // Customer Management
        Task<Customer> CreateOrUpdateCustomerAsync(Customer customer);
        Task<Customer?> GetCustomerAsync(int customerId);
        Task<IEnumerable<Customer>> GetCustomersByDealerAsync(int dealerId);

        // Quotation Management
        Task<Quotation> CreateQuotationAsync(int customerId, int variantId, int dealerId, decimal price);
        Task<Quotation> CreateQuotationAsync(Quotation quotation);
        Task<Quotation?> GetQuotationAsync(int quotationId);
        Task<Quotation?> GetQuotationByIdAsync(int quotationId);
        Task<Quotation> ApproveQuotationAsync(int quotationId);
        Task<Quotation> RejectQuotationAsync(int quotationId);
        Task<IEnumerable<Quotation>> GetPendingQuotationsAsync(int dealerId);
        Task<IEnumerable<Quotation>> GetQuotationsByDealerAsync(int dealerId);
        Task UpdateQuotationStatusAsync(int quotationId, string status);

        // Order Management
        Task<Order> CreateOrderFromQuotationAsync(int quotationId);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order?> GetOrderAsync(int orderId);
        Task<Order> UpdateOrderStatusAsync(int orderId, string status);
        Task<IEnumerable<Order>> GetOrdersByDealerAsync(int dealerId);

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