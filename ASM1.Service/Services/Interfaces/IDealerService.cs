using ASM1.Repository.Models;

namespace ASM1.Service.Services.Interfaces
{
    public interface IDealerService
    {
        // Dealer Management
        Task<IEnumerable<Dealer>> GetAllDealersAsync();
        Task<Dealer?> GetDealerByIdAsync(int dealerId);
        Task<Dealer> CreateDealerAsync(Dealer dealer);
        Task<Dealer> UpdateDealerAsync(Dealer dealer);

        // Dealer Contract Management
        Task<IEnumerable<DealerContract>> GetDealerContractsAsync(int dealerId);
        Task<DealerContract?> GetDealerContractByIdAsync(int contractId);
        Task<DealerContract> CreateDealerContractAsync(DealerContract contract);
        Task<DealerContract> UpdateDealerContractAsync(DealerContract contract);

        // Business Logic Methods
        Task<bool> IsDealerActiveAsync(int dealerId);
        Task<IEnumerable<Dealer>> GetDealersByRegionAsync(string region);
        Task<bool> CanDeleteDealerAsync(int dealerId);
        Task<decimal> GetDealerTotalSalesAsync(int dealerId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}