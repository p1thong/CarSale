using ASM1.Repository.Models;

namespace ASM1.Repository.Repositories.Interfaces
{
    public interface IDealerRepository
    {
        Task<IEnumerable<Dealer>> GetAllDealersAsync();
        Task<Dealer?> GetDealerByIdAsync(int dealerId);
        Task<Dealer?> GetDealerByEmailAsync(string email);
        Task<Dealer> CreateDealerAsync(Dealer dealer);
        Task<Dealer> UpdateDealerAsync(Dealer dealer);
        Task<bool> DeleteDealerAsync(int dealerId);

        // Dealer Contract Management
        Task<IEnumerable<DealerContract>> GetDealerContractsAsync(int dealerId);
        Task<DealerContract> CreateDealerContractAsync(DealerContract contract);
        Task<DealerContract> UpdateDealerContractAsync(DealerContract contract);
    }
}