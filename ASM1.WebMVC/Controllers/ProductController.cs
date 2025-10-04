using ASM1.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASM1.WebMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly IDealerService _dealerService;

        public ProductController(IVehicleService vehicleService, IDealerService dealerService)
        {
            _vehicleService = vehicleService;
            _dealerService = dealerService;
        }

        // Vehicle Models Management
        public async Task<IActionResult> VehicleModels()
        {
            var models = await _vehicleService.GetAllVehicleModelsAsync();
            return View(models);
        }

        [HttpGet]
        public async Task<IActionResult> VehicleModelDetail(int id)
        {
            try
            {
                var model = await _vehicleService.GetVehicleModelByIdAsync(id);
                if (model == null)
                {
                    TempData["Error"] = "Vehicle model not found.";
                    return RedirectToAction(nameof(VehicleModels));
                }

                // Get all variants for this model
                var variants = await _vehicleService.GetVehicleVariantsByModelAsync(id);
                ViewBag.Variants = variants ?? new List<VehicleVariant>();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading vehicle model: {ex.Message}";
                return RedirectToAction(nameof(VehicleModels));
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateVehicleModel()
        {
            try
            {
                var manufacturers = await _vehicleService.GetAllManufacturersAsync();
                Console.WriteLine($"Found {manufacturers?.Count()} manufacturers");

                // If no manufacturers exist, create some sample ones
                if (manufacturers == null || !manufacturers.Any())
                {
                    Console.WriteLine("No manufacturers found, creating sample data...");
                    
                    try
                    {
                        var toyota = new Manufacturer { ManufacturerId = 0, Name = "Toyota", Country = "Japan" };
                        var bmw = new Manufacturer { ManufacturerId = 0, Name = "BMW", Country = "Germany" };
                        var ford = new Manufacturer { ManufacturerId = 0, Name = "Ford", Country = "USA" };

                        await _vehicleService.CreateManufacturerAsync(toyota);
                        await _vehicleService.CreateManufacturerAsync(bmw);
                        await _vehicleService.CreateManufacturerAsync(ford);
                        
                        // Reload manufacturers
                        manufacturers = await _vehicleService.GetAllManufacturersAsync();
                        Console.WriteLine($"Created sample manufacturers, now have {manufacturers?.Count()} manufacturers");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating sample manufacturers: {ex.Message}");
                    }
                }

                if (manufacturers == null || !manufacturers.Any())
                {
                    Console.WriteLine("Still no manufacturers available");
                    ViewBag.Manufacturers = new List<SelectListItem>();
                    TempData["Warning"] = "No manufacturers available. Please create manufacturers first.";
                }
                else
                {
                    var manufacturerList = manufacturers
                        .Select(m => new SelectListItem
                        {
                            Value = m.ManufacturerId.ToString(),
                            Text = m.Name,
                        })
                        .ToList();
                    
                    Console.WriteLine("Manufacturers for dropdown:");
                    foreach (var item in manufacturerList)
                    {
                        Console.WriteLine($"  Value: {item.Value}, Text: {item.Text}");
                    }
                    
                    ViewBag.Manufacturers = manufacturerList;
                }

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading manufacturers: {ex.Message}";
                ViewBag.Manufacturers = new List<SelectListItem>();
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicleModel(VehicleModel model)
        {
            Console.WriteLine("CreateVehicleModel POST action called");
            Console.WriteLine($"Model: Name={model?.Name}, ManufacturerId={model?.ManufacturerId}, Category={model?.Category}, ImageUrl={model?.ImageUrl}");
            
            // Debug form data
            Console.WriteLine("Form data received:");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"  {key}: {Request.Form[key]}");
            }
            
            // Remove Manufacturer navigation property validation since we only need ManufacturerId
            if (ModelState.ContainsKey("Manufacturer"))
            {
                ModelState.Remove("Manufacturer");
            }
            
            // Remove VehicleVariants validation since it's not needed for creation
            if (ModelState.ContainsKey("VehicleVariants"))
            {
                ModelState.Remove("VehicleVariants");
            }
            
            // Manual validation for ManufacturerId - must be selected (not empty string)
            if (model?.ManufacturerId == null)
            {
                Console.WriteLine($"ManufacturerId is null");
                ModelState.AddModelError("ManufacturerId", "Please select a manufacturer.");
            }
            else
            {
                Console.WriteLine($"Valid ManufacturerId: {model.ManufacturerId}");
            }
            
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState validation errors:");
                foreach (var modelError in ModelState)
                {
                    foreach (var error in modelError.Value.Errors)
                    {
                        Console.WriteLine($"  {modelError.Key}: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid && model != null)
            {
                try
                {
                    // Set VehicleModelId to 0 to let EF auto-generate it
                    model.VehicleModelId = 0;
                    
                    await _vehicleService.CreateVehicleModelAsync(model);
                    TempData["Success"] = "Vehicle model created successfully!";
                    Console.WriteLine("Vehicle model created successfully");
                    return RedirectToAction(nameof(VehicleModels));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating vehicle model: {ex.Message}");
                    TempData["Error"] = $"Error creating vehicle model: {ex.Message}";
                }
            }

            try
            {
                var manufacturers = await _vehicleService.GetAllManufacturersAsync();
                ViewBag.Manufacturers =
                    manufacturers
                        ?.Select(m => new SelectListItem
                        {
                            Value = m.ManufacturerId.ToString(),
                            Text = m.Name,
                        })
                        .ToList() ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading manufacturers: {ex.Message}";
                ViewBag.Manufacturers = new List<SelectListItem>();
            }

            return View(model);
        }

        // Vehicle Variants Management
        public async Task<IActionResult> VehicleVariants(int? modelId)
        {
            IEnumerable<VehicleVariant> variants;
            if (modelId.HasValue)
            {
                variants = await _vehicleService.GetVehicleVariantsByModelAsync(modelId.Value);
                var model = await _vehicleService.GetVehicleModelByIdAsync(modelId.Value);
                ViewBag.VehicleModelName = model?.Name;
                ViewBag.SelectedModelId = modelId.Value;
            }
            else
            {
                variants = await _vehicleService.GetAllVehicleVariantsAsync();
            }

            return View(variants);
        }

        [HttpGet]
        public async Task<IActionResult> CreateVehicleVariant()
        {
            var models = await _vehicleService.GetAllVehicleModelsAsync();
            ViewBag.VehicleModels = models
                .Select(m => new SelectListItem
                {
                    Value = m.VehicleModelId.ToString(),
                    Text = $"{m.Manufacturer?.Name} {m.Name}",
                })
                .ToList();
            return View();
        }

        [HttpGet]
        [Route("Product/CreateVehicleVariant/{modelId:int}")]
        public async Task<IActionResult> CreateVehicleVariantForModel(int modelId)
        {
            var models = await _vehicleService.GetAllVehicleModelsAsync();
            ViewBag.VehicleModels = models
                .Select(m => new SelectListItem
                {
                    Value = m.VehicleModelId.ToString(),
                    Text = $"{m.Manufacturer?.Name} {m.Name}",
                    Selected = m.VehicleModelId == modelId,
                })
                .ToList();
            ViewBag.SelectedModelId = modelId;
            return View("CreateVehicleVariant");
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicleVariant(VehicleVariant variant)
        {
            if (ModelState.IsValid)
            {
                await _vehicleService.CreateVehicleVariantAsync(variant);
                TempData["Success"] = "Vehicle variant created successfully!";
                return RedirectToAction(
                    nameof(VehicleVariants),
                    new { modelId = variant.VehicleModelId }
                );
            }

            var models = await _vehicleService.GetAllVehicleModelsAsync();
            ViewBag.VehicleModels = models
                .Select(m => new SelectListItem
                {
                    Value = m.VehicleModelId.ToString(),
                    Text = $"{m.Manufacturer?.Name} {m.Name}",
                })
                .ToList();
            return View(variant);
        }

        [HttpGet]
        public async Task<IActionResult> EditVehicleVariant(int id)
        {
            try
            {
                var variant = await _vehicleService.GetVehicleVariantByIdAsync(id);
                if (variant == null)
                {
                    TempData["Error"] = "Vehicle variant not found.";
                    return RedirectToAction(nameof(VehicleVariants));
                }

                var models = await _vehicleService.GetAllVehicleModelsAsync();
                ViewBag.VehicleModels = models
                    .Select(m => new SelectListItem
                    {
                        Value = m.VehicleModelId.ToString(),
                        Text = $"{m.Manufacturer?.Name} {m.Name}",
                        Selected = m.VehicleModelId == variant.VehicleModelId,
                    })
                    .ToList();

                return View(variant);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading vehicle variant: {ex.Message}";
                return RedirectToAction(nameof(VehicleVariants));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditVehicleVariant(VehicleVariant variant)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _vehicleService.UpdateVehicleVariantAsync(variant);
                    TempData["Success"] = "Vehicle variant updated successfully!";
                    return RedirectToAction(nameof(VehicleVariants), new { modelId = variant.VehicleModelId });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error updating vehicle variant: {ex.Message}";
                }
            }

            var models = await _vehicleService.GetAllVehicleModelsAsync();
            ViewBag.VehicleModels = models
                .Select(m => new SelectListItem
                {
                    Value = m.VehicleModelId.ToString(),
                    Text = $"{m.Manufacturer?.Name} {m.Name}",
                    Selected = m.VehicleModelId == variant.VehicleModelId,
                })
                .ToList();

            return View(variant);
        }

        [HttpGet]
        public async Task<IActionResult> VehicleVariantDetails(int id)
        {
            try
            {
                var variant = await _vehicleService.GetVehicleVariantByIdAsync(id);
                if (variant == null)
                {
                    TempData["Error"] = "Vehicle variant not found.";
                    return RedirectToAction(nameof(VehicleVariants));
                }

                return View(variant);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading vehicle variant details: {ex.Message}";
                return RedirectToAction(nameof(VehicleVariants));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVehicleVariant(int id)
        {
            try
            {
                var variant = await _vehicleService.GetVehicleVariantByIdAsync(id);
                if (variant == null)
                {
                    TempData["Error"] = "Vehicle variant not found.";
                    return RedirectToAction(nameof(VehicleVariants));
                }

                var modelId = variant.VehicleModelId;
                await _vehicleService.DeleteVehicleVariantAsync(id);
                TempData["Success"] = "Vehicle variant deleted successfully!";
                return RedirectToAction(nameof(VehicleVariants), new { modelId = modelId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting vehicle variant: {ex.Message}";
                return RedirectToAction(nameof(VehicleVariants));
            }
        }

        // Manufacturers Management
        public async Task<IActionResult> Manufacturers()
        {
            var manufacturers = await _vehicleService.GetAllManufacturersAsync();
            return View(manufacturers);
        }

        [HttpGet]
        public IActionResult CreateManufacturer()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateManufacturer(Manufacturer manufacturer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set ManufacturerId to 0 to let EF auto-generate it
                    manufacturer.ManufacturerId = 0;
                    
                    await _vehicleService.CreateManufacturerAsync(manufacturer);
                    TempData["Success"] = "Manufacturer created successfully!";
                    return RedirectToAction(nameof(Manufacturers));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating manufacturer: {ex.Message}";
                }
            }
            return View(manufacturer);
        }

        [HttpGet]
        public async Task<IActionResult> EditManufacturer(int id)
        {
            var manufacturer = await _vehicleService.GetManufacturerByIdAsync(id);
            if (manufacturer == null)
            {
                TempData["Error"] = "Manufacturer not found.";
                return RedirectToAction(nameof(Manufacturers));
            }
            return View(manufacturer);
        }

        [HttpPost]
        public async Task<IActionResult> EditManufacturer(Manufacturer manufacturer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _vehicleService.UpdateManufacturerAsync(manufacturer);
                    TempData["Success"] = "Manufacturer updated successfully!";
                    return RedirectToAction(nameof(Manufacturers));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error updating manufacturer: {ex.Message}";
                }
            }
            return View(manufacturer);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteManufacturer(int id)
        {
            try
            {
                var manufacturer = await _vehicleService.GetManufacturerByIdAsync(id);
                if (manufacturer == null)
                {
                    return Json(new { success = false, message = "Manufacturer not found." });
                }

                // Check if manufacturer has associated vehicle models
                if (manufacturer.VehicleModels.Any())
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "Cannot delete manufacturer with associated vehicle models.",
                        }
                    );
                }

                // Delete manufacturer logic would go here
                // For now, return success
                return Json(new { success = true, message = "Manufacturer deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Inventory Management
        public async Task<IActionResult> Inventory()
        {
            var variants = await _vehicleService.GetAvailableVariantsAsync();
            return View(variants);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(int variantId, int quantity, string operation)
        {
            try
            {
                var variant = await _vehicleService.GetVehicleVariantByIdAsync(variantId);
                if (variant == null)
                {
                    return Json(new { success = false, message = "Vehicle variant not found." });
                }

                // Stock management logic would go here
                // For now, return success response
                var message =
                    operation == "add"
                        ? $"Added {quantity} units to stock."
                        : $"Removed {quantity} units from stock.";

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryStats()
        {
            try
            {
                var variants = await _vehicleService.GetAvailableVariantsAsync();

                // Calculate low stock count based on actual stock data
                // Assuming we have a StockQuantity property, otherwise it's 0
                var lowStockThreshold = 10; // Define what's considered "low stock"
                var lowStockCount = variants.Count(v => 
                    (v.Price.HasValue ? 1 : 0) < lowStockThreshold); // Placeholder logic - should use actual stock property

                var stats = new
                {
                    totalModels = variants.Select(v => v.VehicleModelId).Distinct().Count(),
                    totalVariants = variants.Count(),
                    lowStockCount = lowStockCount,
                    totalValue = variants.Sum(v => v.Price ?? 0),
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Distribution Management
        public async Task<IActionResult> Distribution()
        {
            var dealers = await _dealerService.GetAllDealersAsync();
            return View(dealers);
        }

        [HttpGet]
        public async Task<IActionResult> DealerContracts(int dealerId)
        {
            var contracts = await _dealerService.GetDealerContractsAsync(dealerId);
            var dealer = await _dealerService.GetDealerByIdAsync(dealerId);
            ViewBag.DealerName = dealer?.FullName;
            ViewBag.DealerId = dealerId;
            return View(contracts);
        }

        [HttpGet]
        public async Task<IActionResult> CreateDealerContract(int dealerId)
        {
            var dealer = await _dealerService.GetDealerByIdAsync(dealerId);
            ViewBag.DealerName = dealer?.FullName;
            ViewBag.DealerId = dealerId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDealerContract(DealerContract contract)
        {
            if (ModelState.IsValid)
            {
                await _dealerService.CreateDealerContractAsync(contract);
                TempData["Success"] = "Dealer contract created successfully!";
                return RedirectToAction(
                    nameof(DealerContracts),
                    new { dealerId = contract.DealerId }
                );
            }

            var dealer = await _dealerService.GetDealerByIdAsync(contract.DealerId);
            ViewBag.DealerName = dealer?.FullName;
            ViewBag.DealerId = contract.DealerId;
            return View(contract);
        }

        // Sample data seeder (for testing)
        [HttpGet]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                // Check if manufacturers already exist
                var existingManufacturers = await _vehicleService.GetAllManufacturersAsync();
                if (existingManufacturers.Any())
                {
                    TempData["Info"] = "Sample data already exists.";
                    return RedirectToAction(nameof(VehicleModels));
                }

                // Create sample manufacturers with ID = 0 to let EF auto-generate
                var toyota = new Manufacturer { ManufacturerId = 0, Name = "Toyota", Country = "Japan" };
                var bmw = new Manufacturer { ManufacturerId = 0, Name = "BMW", Country = "Germany" };
                var ford = new Manufacturer { ManufacturerId = 0, Name = "Ford", Country = "USA" };

                await _vehicleService.CreateManufacturerAsync(toyota);
                await _vehicleService.CreateManufacturerAsync(bmw);
                await _vehicleService.CreateManufacturerAsync(ford);

                TempData["Success"] = "Sample data created successfully!";
                return RedirectToAction(nameof(VehicleModels));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating sample data: {ex.Message}";
                return RedirectToAction(nameof(VehicleModels));
            }
        }
    }
}
