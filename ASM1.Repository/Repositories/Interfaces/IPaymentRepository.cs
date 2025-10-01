using ASM1.Repository.Models;

namespace ASM1.Repository.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<IEnumerable<Payment>> GetPaymentsByOrderAsync(int orderId);
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<Payment> UpdatePaymentAsync(Payment payment);
        Task<bool> DeletePaymentAsync(int paymentId);
        Task<decimal> GetTotalPaidAmountByOrderAsync(int orderId);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
    }
}