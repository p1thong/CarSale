using ASM1.Repository.Data;
using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASM1.Repository.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly CarSalesDbContext _context;

        public VehicleRepository(CarSalesDbContext context)
        {
            _context = context;
        }

        // Vehicle Model Management
        public async Task<IEnumerable<VehicleModel>> GetAllVehicleModelsAsync()
        {
            return await _context
                .VehicleModels.Include(m => m.Manufacturer)
                .Include(m => m.VehicleVariants)
                .ToListAsync();
        }

        public async Task<VehicleModel?> GetVehicleModelByIdAsync(int modelId)
        {
            return await _context
                .VehicleModels.Include(m => m.Manufacturer)
                .Include(m => m.VehicleVariants)
                .FirstOrDefaultAsync(m => m.VehicleModelId == modelId);
        }

        public async Task<IEnumerable<VehicleModel>> GetVehicleModelsByManufacturerAsync(
            int manufacturerId
        )
        {
            return await _context
                .VehicleModels.AsNoTracking()
                .Where(m => m.ManufacturerId == manufacturerId)
                .Include(m => m.Manufacturer)
                .Include(m => m.VehicleVariants)
                .ToListAsync();
        }

        public async Task<VehicleModel> CreateVehicleModelAsync(VehicleModel model)
        {
            _context.VehicleModels.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<VehicleModel> UpdateVehicleModelAsync(VehicleModel model)
        {
            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return model;
        }

        // Vehicle Variant Management
        public async Task<IEnumerable<VehicleVariant>> GetAllVehicleVariantsAsync()
        {
            return await _context
                .VehicleVariants.Include(v => v.VehicleModel)
                .ThenInclude(m => m.Manufacturer)
                .ToListAsync();
        }

        public async Task<VehicleVariant?> GetVehicleVariantByIdAsync(int variantId)
        {
            return await _context
                .VehicleVariants.Include(v => v.VehicleModel)
                .ThenInclude(m => m.Manufacturer)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);
        }

        public async Task<IEnumerable<VehicleVariant>> GetVehicleVariantsByModelAsync(int modelId)
        {
            return await _context
                .VehicleVariants.Where(v => v.VehicleModelId == modelId)
                .Include(v => v.VehicleModel)
                .ToListAsync();
        }

        public async Task<VehicleVariant> CreateVehicleVariantAsync(VehicleVariant variant)
        {
            _context.VehicleVariants.Add(variant);
            await _context.SaveChangesAsync();
            return variant;
        }

        public async Task<VehicleVariant> UpdateVehicleVariantAsync(VehicleVariant variant)
        {
            _context.Entry(variant).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return variant;
        }

        public async Task DeleteVehicleVariantAsync(int variantId)
        {
            var variant = await _context.VehicleVariants.FindAsync(variantId);
            if (variant != null)
            {
                _context.VehicleVariants.Remove(variant);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<VehicleVariant>> GetAvailableVariantsAsync()
        {
            return await _context
                .VehicleVariants.Where(v => v.Price.HasValue && v.Price > 0)
                .Include(v => v.VehicleModel)
                .ThenInclude(m => m.Manufacturer)
                .ToListAsync();
        }

        // Manufacturer Management
        public async Task<IEnumerable<Manufacturer>> GetAllManufacturersAsync()
        {
            return await _context.Manufacturers.Include(m => m.VehicleModels).ToListAsync();
        }

        public async Task<Manufacturer?> GetManufacturerByIdAsync(int manufacturerId)
        {
            return await _context
                .Manufacturers.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ManufacturerId == manufacturerId);
        }

        public async Task<Manufacturer> CreateManufacturerAsync(Manufacturer manufacturer)
        {
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();
            return manufacturer;
        }

        public async Task<bool> ManufacturerExistsAsync(int manufacturerId)
        {
            return await _context
                .Manufacturers.AsNoTracking()
                .AnyAsync(m => m.ManufacturerId == manufacturerId);
        }

        public async Task<bool> VehicleModelNameExistsAsync(
            string name,
            int manufacturerId,
            int? excludeId = null
        )
        {
            return await _context
                .VehicleModels.AsNoTracking()
                .AnyAsync(m =>
                    m.Name.Equals(name)
                    && m.ManufacturerId == manufacturerId
                    && (excludeId == null || m.VehicleModelId != excludeId)
                );
        }

        public async Task<Manufacturer> UpdateManufacturerAsync(Manufacturer manufacturer)
        {
            _context.Entry(manufacturer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return manufacturer;
        }

        public async Task DeleteManufacturerAsync(int manufacturerId)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(manufacturerId);
            if (manufacturer != null)
            {
                _context.Manufacturers.Remove(manufacturer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteVehicleModelAsync(int modelId)
        {
            var model = await _context.VehicleModels.FindAsync(modelId);
            if (model != null)
            {
                _context.VehicleModels.Remove(model);
                await _context.SaveChangesAsync();
            }
        }
    }
}
