using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using ASM1.Service.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASM1.Service.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        // Manufacturer Management
        public async Task<IEnumerable<Manufacturer>> GetAllManufacturersAsync()
        {
            return await _vehicleRepository.GetAllManufacturersAsync();
        }

        public async Task<Manufacturer?> GetManufacturerByIdAsync(int manufacturerId)
        {
            return await _vehicleRepository.GetManufacturerByIdAsync(manufacturerId);
        }

        public async Task<Manufacturer> CreateManufacturerAsync(Manufacturer manufacturer)
        {
            // Validate manufacturer name uniqueness
            if (await IsManufacturerNameExistsAsync(manufacturer.Name))
            {
                throw new InvalidOperationException(
                    $"Manufacturer with name '{manufacturer.Name}' already exists."
                );
            }

            return await _vehicleRepository.CreateManufacturerAsync(manufacturer);
        }

        public async Task<Manufacturer> UpdateManufacturerAsync(Manufacturer manufacturer)
        {
            // Validate manufacturer exists
            var existingManufacturer = await _vehicleRepository.GetManufacturerByIdAsync(
                manufacturer.ManufacturerId
            );
            if (existingManufacturer == null)
            {
                throw new ArgumentException("Manufacturer not found.");
            }

            // Validate manufacturer name uniqueness (excluding current manufacturer)
            if (await IsManufacturerNameExistsAsync(manufacturer.Name, manufacturer.ManufacturerId))
            {
                throw new InvalidOperationException(
                    $"Another manufacturer with name '{manufacturer.Name}' already exists."
                );
            }

            // Update properties
            existingManufacturer.Name = manufacturer.Name;
            existingManufacturer.Country = manufacturer.Country;
            existingManufacturer.Address = manufacturer.Address;

            return await _vehicleRepository.UpdateManufacturerAsync(existingManufacturer);
        }

        // Vehicle Model Management
        public async Task<IEnumerable<VehicleModel>> GetAllVehicleModelsAsync()
        {
            return await _vehicleRepository.GetAllVehicleModelsAsync();
        }

        public async Task<VehicleModel?> GetVehicleModelByIdAsync(int modelId)
        {
            return await _vehicleRepository.GetVehicleModelByIdAsync(modelId);
        }

        public async Task<IEnumerable<VehicleModel>> GetVehicleModelsByManufacturerAsync(
            int manufacturerId
        )
        {
            return await _vehicleRepository.GetVehicleModelsByManufacturerAsync(manufacturerId);
        }

        public async Task<VehicleModel> CreateVehicleModelAsync(VehicleModel model)
        {
            // Validate manufacturer exists
            if (!await _vehicleRepository.ManufacturerExistsAsync(model.ManufacturerId))
            {
                throw new ArgumentException("Manufacturer not found.");
            }

            // Temporarily disable name validation to test form submission
            // if (await IsVehicleModelNameExistsAsync(model.Name, model.ManufacturerId))
            // {
            //     throw new InvalidOperationException($"A vehicle model with name '{model.Name}' already exists for this manufacturer.");
            // }

            return await _vehicleRepository.CreateVehicleModelAsync(model);
        }

        public async Task<VehicleModel> UpdateVehicleModelAsync(VehicleModel model)
        {
            // Validate model exists
            var existingModel = await _vehicleRepository.GetVehicleModelByIdAsync(
                model.VehicleModelId
            );
            if (existingModel == null)
            {
                throw new ArgumentException("Vehicle model not found.");
            }

            // Validate manufacturer exists
            var manufacturer = await _vehicleRepository.GetManufacturerByIdAsync(
                model.ManufacturerId
            );
            if (manufacturer == null)
            {
                throw new ArgumentException("Manufacturer not found.");
            }

            // Validate model name uniqueness within manufacturer (excluding current model)
            if (
                await IsVehicleModelNameExistsAsync(
                    model.Name,
                    model.ManufacturerId,
                    model.VehicleModelId
                )
            )
            {
                throw new InvalidOperationException(
                    $"Another vehicle model with name '{model.Name}' already exists for this manufacturer."
                );
            }

            // Update properties
            existingModel.Name = model.Name;
            existingModel.ManufacturerId = model.ManufacturerId;
            existingModel.Category = model.Category;

            return await _vehicleRepository.UpdateVehicleModelAsync(existingModel);
        }

        // Vehicle Variant Management
        public async Task<IEnumerable<VehicleVariant>> GetAllVehicleVariantsAsync()
        {
            return await _vehicleRepository.GetAllVehicleVariantsAsync();
        }

        public async Task<VehicleVariant?> GetVehicleVariantByIdAsync(int variantId)
        {
            return await _vehicleRepository.GetVehicleVariantByIdAsync(variantId);
        }

        public async Task<IEnumerable<VehicleVariant>> GetVehicleVariantsByModelAsync(int modelId)
        {
            return await _vehicleRepository.GetVehicleVariantsByModelAsync(modelId);
        }

        public async Task<VehicleVariant> CreateVehicleVariantAsync(VehicleVariant variant)
        {
            // Validate vehicle model exists
            var vehicleModel = await _vehicleRepository.GetVehicleModelByIdAsync(
                variant.VehicleModelId
            );
            if (vehicleModel == null)
            {
                throw new ArgumentException("Vehicle model not found.");
            }

            return await _vehicleRepository.CreateVehicleVariantAsync(variant);
        }

        public async Task<VehicleVariant> UpdateVehicleVariantAsync(VehicleVariant variant)
        {
            // Validate variant exists
            var existingVariant = await _vehicleRepository.GetVehicleVariantByIdAsync(
                variant.VariantId
            );
            if (existingVariant == null)
            {
                throw new ArgumentException("Vehicle variant not found.");
            }

            // Validate vehicle model exists
            var vehicleModel = await _vehicleRepository.GetVehicleModelByIdAsync(
                variant.VehicleModelId
            );
            if (vehicleModel == null)
            {
                throw new ArgumentException("Vehicle model not found.");
            }

            // Update properties
            existingVariant.VehicleModelId = variant.VehicleModelId;
            existingVariant.Version = variant.Version;
            existingVariant.Color = variant.Color;
            existingVariant.ProductYear = variant.ProductYear;
            existingVariant.Price = variant.Price;

            return await _vehicleRepository.UpdateVehicleVariantAsync(existingVariant);
        }

        public async Task DeleteVehicleVariantAsync(int variantId)
        {
            // Validate variant exists
            var existingVariant = await _vehicleRepository.GetVehicleVariantByIdAsync(variantId);
            if (existingVariant == null)
            {
                throw new ArgumentException("Vehicle variant not found.");
            }

            // TODO: Add business logic validation (check if variant is used in orders, quotations, etc.)

            await _vehicleRepository.DeleteVehicleVariantAsync(variantId);
        }

        public async Task<IEnumerable<VehicleVariant>> GetAvailableVariantsAsync()
        {
            return await _vehicleRepository.GetAvailableVariantsAsync();
        }

        // Business Logic Methods
        public async Task<bool> IsManufacturerNameExistsAsync(string name, int? excludeId = null)
        {
            var manufacturers = await _vehicleRepository.GetAllManufacturersAsync();
            return manufacturers.Any(m =>
                m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                && (excludeId == null || m.ManufacturerId != excludeId)
            );
        }

        public async Task<bool> IsVehicleModelNameExistsAsync(
            string name,
            int manufacturerId,
            int? excludeId = null
        )
        {
            return await _vehicleRepository.VehicleModelNameExistsAsync(
                name,
                manufacturerId,
                excludeId
            );
        }

        public async Task<bool> CanDeleteManufacturerAsync(int manufacturerId)
        {
            var models = await _vehicleRepository.GetVehicleModelsByManufacturerAsync(
                manufacturerId
            );
            return !models.Any();
        }

        public async Task DeleteManufacturerAsync(int manufacturerId)
        {
            // Check if can delete first
            var canDelete = await CanDeleteManufacturerAsync(manufacturerId);
            if (!canDelete)
            {
                throw new InvalidOperationException("Cannot delete manufacturer that has vehicle models associated with it.");
            }

            await _vehicleRepository.DeleteManufacturerAsync(manufacturerId);
        }

        public async Task<bool> CanDeleteVehicleModelAsync(int modelId)
        {
            var variants = await _vehicleRepository.GetVehicleVariantsByModelAsync(modelId);
            return !variants.Any();
        }

        public async Task DeleteVehicleModelAsync(int modelId)
        {
            // Check if can delete first
            var canDelete = await CanDeleteVehicleModelAsync(modelId);
            if (!canDelete)
            {
                throw new InvalidOperationException("Cannot delete vehicle model that has variants associated with it.");
            }

            await _vehicleRepository.DeleteVehicleModelAsync(modelId);
        }

        public async Task<IEnumerable<VehicleModel>> SearchVehicleModelsAsync(string searchTerm)
        {
            var allModels = await _vehicleRepository.GetAllVehicleModelsAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
                return allModels;

            return allModels.Where(m =>
                m.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || (m.Category?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                || m.Manufacturer.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            );
        }

        public async Task<IEnumerable<VehicleVariant>> SearchVehicleVariantsAsync(string searchTerm)
        {
            var allVariants = await _vehicleRepository.GetAllVehicleVariantsAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
                return allVariants;

            return allVariants.Where(v =>
                v.Version.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || (v.Color?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                || v.VehicleModel.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}
