using ASM1.Repository.Models;

namespace ASM1.Repository.Repositories.Interfaces
{
    public interface IVehicleRepository
    {
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

        // Manufacturer Management
        Task<IEnumerable<Manufacturer>> GetAllManufacturersAsync();
        Task<Manufacturer?> GetManufacturerByIdAsync(int manufacturerId);
        Task<Manufacturer> CreateManufacturerAsync(Manufacturer manufacturer);
        Task<Manufacturer> UpdateManufacturerAsync(Manufacturer manufacturer);
        Task DeleteManufacturerAsync(int manufacturerId);
        Task DeleteVehicleModelAsync(int modelId);
        Task<bool> ManufacturerExistsAsync(int manufacturerId);
        Task<bool> VehicleModelNameExistsAsync(string name, int manufacturerId, int? excludeId = null);
    }
}