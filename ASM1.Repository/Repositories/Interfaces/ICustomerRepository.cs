using ASM1.Repository.Models;

namespace ASM1.Repository.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task<IEnumerable<Customer>> GetCustomersByDealerAsync(int dealerId);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int customerId);
        Task<bool> CustomerExistsAsync(int customerId);
    }
}