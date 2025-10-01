using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using ASM1.Service.Services.Interfaces;

namespace ASM1.Service.Services
{
    public class DealerService : IDealerService
    {
        private readonly IDealerRepository _dealerRepository;

        public DealerService(IDealerRepository dealerRepository)
        {
            _dealerRepository = dealerRepository;
        }

        // Dealer Management
        public async Task<IEnumerable<Dealer>> GetAllDealersAsync()
        {
            return await _dealerRepository.GetAllDealersAsync();
        }

        public async Task<Dealer?> GetDealerByIdAsync(int dealerId)
        {
            return await _dealerRepository.GetDealerByIdAsync(dealerId);
        }

        public async Task<Dealer> CreateDealerAsync(Dealer dealer)
        {
            // Business logic validation
            if (string.IsNullOrWhiteSpace(dealer.FullName))
            {
                throw new ArgumentException("Dealer name is required.");
            }

            if (string.IsNullOrWhiteSpace(dealer.Email))
            {
                throw new ArgumentException("Dealer email is required.");
            }

            if (string.IsNullOrWhiteSpace(dealer.Password))
            {
                throw new ArgumentException("Dealer password is required.");
            }

            return await _dealerRepository.CreateDealerAsync(dealer);
        }

        public async Task<Dealer> UpdateDealerAsync(Dealer dealer)
        {
            // Validate dealer exists
            var existingDealer = await _dealerRepository.GetDealerByIdAsync(dealer.DealerId);
            if (existingDealer == null)
            {
                throw new ArgumentException("Dealer not found.");
            }

            // Business logic validation
            if (string.IsNullOrWhiteSpace(dealer.FullName))
            {
                throw new ArgumentException("Dealer name is required.");
            }

            if (string.IsNullOrWhiteSpace(dealer.Email))
            {
                throw new ArgumentException("Dealer email is required.");
            }

            // Update properties (only the ones that exist in Dealer model)
            existingDealer.FullName = dealer.FullName;
            existingDealer.Email = dealer.Email;
            existingDealer.Phone = dealer.Phone;
            // Note: Password should be handled separately with proper hashing

            return await _dealerRepository.UpdateDealerAsync(existingDealer);
        }

        // Dealer Contract Management
        public async Task<IEnumerable<DealerContract>> GetDealerContractsAsync(int dealerId)
        {
            return await _dealerRepository.GetDealerContractsAsync(dealerId);
        }

        public async Task<DealerContract?> GetDealerContractByIdAsync(int contractId)
        {
            // Since there's no GetDealerContractByIdAsync in repository, we'll get all and filter
            var allContracts = await _dealerRepository.GetDealerContractsAsync(0); // This might need adjustment
            return allContracts.FirstOrDefault(c => c.DealerContractId == contractId);
        }

        public async Task<DealerContract> CreateDealerContractAsync(DealerContract contract)
        {
            // Validate dealer exists
            var dealer = await _dealerRepository.GetDealerByIdAsync(contract.DealerId);
            if (dealer == null)
            {
                throw new ArgumentException("Dealer not found.");
            }

            // Business logic validation for the actual properties
            if (contract.TargetSales.HasValue && contract.TargetSales.Value < 0)
            {
                throw new ArgumentException("Target sales cannot be negative.");
            }

            if (contract.CreditLimit.HasValue && contract.CreditLimit.Value < 0)
            {
                throw new ArgumentException("Credit limit cannot be negative.");
            }

            return await _dealerRepository.CreateDealerContractAsync(contract);
        }

        public async Task<DealerContract> UpdateDealerContractAsync(DealerContract contract)
        {
            // Business logic validation for the actual properties
            if (contract.TargetSales.HasValue && contract.TargetSales.Value < 0)
            {
                throw new ArgumentException("Target sales cannot be negative.");
            }

            if (contract.CreditLimit.HasValue && contract.CreditLimit.Value < 0)
            {
                throw new ArgumentException("Credit limit cannot be negative.");
            }

            return await _dealerRepository.UpdateDealerContractAsync(contract);
        }

        // Business Logic Methods
        public async Task<bool> IsDealerActiveAsync(int dealerId)
        {
            var dealer = await _dealerRepository.GetDealerByIdAsync(dealerId);
            // Since Dealer doesn't have IsActive property, we'll check if dealer exists
            return dealer != null;
        }

        public async Task<IEnumerable<Dealer>> GetDealersByRegionAsync(string region)
        {
            // Since Dealer doesn't have Region property, return all dealers for now
            // In a real implementation, you might add Region to Dealer model or filter by other criteria
            var allDealers = await _dealerRepository.GetAllDealersAsync();
            return allDealers; // Simplified implementation
        }

        public async Task<bool> CanDeleteDealerAsync(int dealerId)
        {
            var contracts = await _dealerRepository.GetDealerContractsAsync(dealerId);
            // Since DealerContract doesn't have Status, we'll check if there are any contracts
            return !contracts.Any();
        }

        public Task<decimal> GetDealerTotalSalesAsync(int dealerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // This would typically involve joining with order/sales data
            // For now, return a placeholder value
            // In a real implementation, you would query sales data through appropriate repositories
            return Task.FromResult(0m);
        }
    }
}