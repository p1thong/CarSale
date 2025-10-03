using ASM1.Repository.Models;

namespace ASM1.Service.Services.Interfaces
{
    public interface IVehicleService
    {
        // Manufacturer Management
        Task<IEnumerable<Manufacturer>> GetAllManufacturersAsync();
        Task<Manufacturer?> GetManufacturerByIdAsync(int manufacturerId);
        Task<Manufacturer> CreateManufacturerAsync(Manufacturer manufacturer);
        Task<Manufacturer> UpdateManufacturerAsync(Manufacturer manufacturer);

        // Vehicle Model Management
        Task<IEnumerable<VehicleModel>> GetAllVehicleModelsAsync();
        Task<VehicleModel?> GetVehicleModelByIdAsync(int modelId);
        Task<IEnumerable<VehicleModel>> GetVehicleModelsByManufacturerAsync(int manufacturerId);
        Task<VehicleModel> CreateVehicleModelAsync(VehicleModel model);
        Task<VehicleModel> UpdateVehicleModelAsync(VehicleModel model);

        // Vehicle Variant Management
        Task<IEnumerable<VehicleVariant>> GetAllVehicleVariantsAsync();
        Task<VehicleVariant?> GetVehicleVariantByIdAsync(int variantId);
        Task<IEnumerable<VehicleVariant>> GetVehicleVariantsByModelAsync(int modelId);
        Task<VehicleVariant> CreateVehicleVariantAsync(VehicleVariant variant);
        Task<VehicleVariant> UpdateVehicleVariantAsync(VehicleVariant variant);
        Task DeleteVehicleVariantAsync(int variantId);
        Task<IEnumerable<VehicleVariant>> GetAvailableVariantsAsync();

        // Business Logic Methods
        Task<bool> IsManufacturerNameExistsAsync(string name, int? excludeId = null);
        Task<bool> IsVehicleModelNameExistsAsync(
            string name,
            int manufacturerId,
            int? excludeId = null
        );
        Task<bool> CanDeleteManufacturerAsync(int manufacturerId);
        Task DeleteManufacturerAsync(int manufacturerId);
        Task<bool> CanDeleteVehicleModelAsync(int modelId);
        Task DeleteVehicleModelAsync(int modelId);
        Task<IEnumerable<VehicleModel>> SearchVehicleModelsAsync(string searchTerm);
        Task<IEnumerable<VehicleVariant>> SearchVehicleVariantsAsync(string searchTerm);
    }
}
