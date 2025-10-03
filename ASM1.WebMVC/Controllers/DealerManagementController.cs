using ASM1.Repository.Models;
using ASM1.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASM1.WebMVC.Controllers
{
    public class DealerManagementController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly ISalesService _salesService;
        private readonly ILogger<DealerManagementController> _logger;

        public DealerManagementController(
            IVehicleService vehicleService,
            ISalesService salesService,
            ILogger<DealerManagementController> logger)
        {
            _vehicleService = vehicleService;
            _salesService = salesService;
            _logger = logger;
        }

        // Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                
                // Lấy dữ liệu cho charts
                var topSellerVehicles = await GetTopSellerVehiclesAsync(dealerId);
                var monthlyRevenue = await GetMonthlyRevenueAsync(dealerId);
                
                ViewBag.TopSellerVehicles = topSellerVehicles;
                ViewBag.MonthlyRevenue = monthlyRevenue;
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dealer dashboard");
                TempData["Error"] = "Lỗi khi tải dashboard: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // === MANUFACTURER CRUD ===
        
        [HttpGet]
        public async Task<IActionResult> Manufacturers()
        {
            try
            {
                var manufacturers = await _vehicleService.GetAllManufacturersAsync();
                return View(manufacturers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manufacturers");
                TempData["Error"] = "Lỗi khi tải danh sách hãng xe: " + ex.Message;
                return View(new List<Manufacturer>());
            }
        }

        [HttpGet]
        public IActionResult CreateManufacturer()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateManufacturer(Manufacturer manufacturer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _vehicleService.CreateManufacturerAsync(manufacturer);
                    TempData["Success"] = "Tạo hãng xe thành công!";
                    return RedirectToAction(nameof(Manufacturers));
                }
                return View(manufacturer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manufacturer");
                TempData["Error"] = "Lỗi khi tạo hãng xe: " + ex.Message;
                return View(manufacturer);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditManufacturer(int id)
        {
            try
            {
                var manufacturer = await _vehicleService.GetManufacturerByIdAsync(id);
                if (manufacturer == null)
                {
                    TempData["Error"] = "Không tìm thấy hãng xe.";
                    return RedirectToAction(nameof(Manufacturers));
                }
                return View(manufacturer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manufacturer {Id}", id);
                TempData["Error"] = "Lỗi khi tải thông tin hãng xe: " + ex.Message;
                return RedirectToAction(nameof(Manufacturers));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditManufacturer(Manufacturer manufacturer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _vehicleService.UpdateManufacturerAsync(manufacturer);
                    TempData["Success"] = "Cập nhật hãng xe thành công!";
                    return RedirectToAction(nameof(Manufacturers));
                }
                return View(manufacturer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating manufacturer {Id}", manufacturer.ManufacturerId);
                TempData["Error"] = "Lỗi khi cập nhật hãng xe: " + ex.Message;
                return View(manufacturer);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteManufacturer(int id)
        {
            try
            {
                await _vehicleService.DeleteManufacturerAsync(id);
                TempData["Success"] = "Xóa hãng xe thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting manufacturer {Id}", id);
                TempData["Error"] = "Lỗi khi xóa hãng xe: " + ex.Message;
            }
            return RedirectToAction(nameof(Manufacturers));
        }

        // === VEHICLE MODEL CRUD ===
        
        [HttpGet]
        public async Task<IActionResult> VehicleModels()
        {
            try
            {
                var models = await _vehicleService.GetAllVehicleModelsAsync();
                return View(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicle models");
                TempData["Error"] = "Lỗi khi tải danh sách mẫu xe: " + ex.Message;
                return View(new List<VehicleModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateVehicleModel()
        {
            try
            {
                ViewBag.Manufacturers = await _vehicleService.GetAllManufacturersAsync();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manufacturers for vehicle model creation");
                TempData["Error"] = "Lỗi khi tải danh sách hãng xe: " + ex.Message;
                return RedirectToAction(nameof(VehicleModels));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicleModel(VehicleModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _vehicleService.CreateVehicleModelAsync(model);
                    TempData["Success"] = "Tạo mẫu xe thành công!";
                    return RedirectToAction(nameof(VehicleModels));
                }
                ViewBag.Manufacturers = await _vehicleService.GetAllManufacturersAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle model");
                TempData["Error"] = "Lỗi khi tạo mẫu xe: " + ex.Message;
                ViewBag.Manufacturers = await _vehicleService.GetAllManufacturersAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditVehicleModel(int id)
        {
            try
            {
                var model = await _vehicleService.GetVehicleModelByIdAsync(id);
                if (model == null)
                {
                    TempData["Error"] = "Không tìm thấy mẫu xe.";
                    return RedirectToAction(nameof(VehicleModels));
                }
                ViewBag.Manufacturers = await _vehicleService.GetAllManufacturersAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicle model {Id}", id);
                TempData["Error"] = "Lỗi khi tải thông tin mẫu xe: " + ex.Message;
                return RedirectToAction(nameof(VehicleModels));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditVehicleModel(VehicleModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _vehicleService.UpdateVehicleModelAsync(model);
                    TempData["Success"] = "Cập nhật mẫu xe thành công!";
                    return RedirectToAction(nameof(VehicleModels));
                }
                ViewBag.Manufacturers = await _vehicleService.GetAllManufacturersAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle model {Id}", model.VehicleModelId);
                TempData["Error"] = "Lỗi khi cập nhật mẫu xe: " + ex.Message;
                ViewBag.Manufacturers = await _vehicleService.GetAllManufacturersAsync();
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVehicleModel(int id)
        {
            try
            {
                await _vehicleService.DeleteVehicleModelAsync(id);
                TempData["Success"] = "Xóa mẫu xe thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle model {Id}", id);
                TempData["Error"] = "Lỗi khi xóa mẫu xe: " + ex.Message;
            }
            return RedirectToAction(nameof(VehicleModels));
        }

        // === VEHICLE VARIANT CRUD ===
        
        [HttpGet]
        public async Task<IActionResult> VehicleVariants()
        {
            try
            {
                var variants = await _vehicleService.GetAllVehicleVariantsAsync();
                return View(variants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicle variants");
                TempData["Error"] = "Lỗi khi tải danh sách phiên bản xe: " + ex.Message;
                return View(new List<VehicleVariant>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateVehicleVariant()
        {
            try
            {
                ViewBag.VehicleModels = await _vehicleService.GetAllVehicleModelsAsync();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicle models for variant creation");
                TempData["Error"] = "Lỗi khi tải danh sách mẫu xe: " + ex.Message;
                return RedirectToAction(nameof(VehicleVariants));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicleVariant(VehicleVariant variant)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _vehicleService.CreateVehicleVariantAsync(variant);
                    TempData["Success"] = "Tạo phiên bản xe thành công!";
                    return RedirectToAction(nameof(VehicleVariants));
                }
                ViewBag.VehicleModels = await _vehicleService.GetAllVehicleModelsAsync();
                return View(variant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle variant");
                TempData["Error"] = "Lỗi khi tạo phiên bản xe: " + ex.Message;
                ViewBag.VehicleModels = await _vehicleService.GetAllVehicleModelsAsync();
                return View(variant);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditVehicleVariant(int id)
        {
            try
            {
                var variant = await _vehicleService.GetVehicleVariantByIdAsync(id);
                if (variant == null)
                {
                    TempData["Error"] = "Không tìm thấy phiên bản xe.";
                    return RedirectToAction(nameof(VehicleVariants));
                }
                ViewBag.VehicleModels = await _vehicleService.GetAllVehicleModelsAsync();
                return View(variant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicle variant {Id}", id);
                TempData["Error"] = "Lỗi khi tải thông tin phiên bản xe: " + ex.Message;
                return RedirectToAction(nameof(VehicleVariants));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditVehicleVariant(VehicleVariant variant)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _vehicleService.UpdateVehicleVariantAsync(variant);
                    TempData["Success"] = "Cập nhật phiên bản xe thành công!";
                    return RedirectToAction(nameof(VehicleVariants));
                }
                ViewBag.VehicleModels = await _vehicleService.GetAllVehicleModelsAsync();
                return View(variant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle variant {Id}", variant.VariantId);
                TempData["Error"] = "Lỗi khi cập nhật phiên bản xe: " + ex.Message;
                ViewBag.VehicleModels = await _vehicleService.GetAllVehicleModelsAsync();
                return View(variant);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVehicleVariant(int id)
        {
            try
            {
                await _vehicleService.DeleteVehicleVariantAsync(id);
                TempData["Success"] = "Xóa phiên bản xe thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle variant {Id}", id);
                TempData["Error"] = "Lỗi khi xóa phiên bản xe: " + ex.Message;
            }
            return RedirectToAction(nameof(VehicleVariants));
        }

        // === HELPER METHODS ===
        
        private int GetCurrentDealerId()
        {
            return HttpContext.Session.GetInt32("DealerId") ?? 1;
        }

        private async Task<List<object>> GetTopSellerVehiclesAsync(int dealerId)
        {
            try
            {
                var orders = await _salesService.GetOrdersByDealerAsync(dealerId);
                var topSellers = orders
                    .Where(o => o.Status == "Delivered")
                    .GroupBy(o => new { o.Variant.VehicleModel.Name, o.Variant.Version })
                    .Select(g => new
                    {
                        VehicleName = $"{g.Key.Name} {g.Key.Version}",
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList<object>();

                return topSellers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top seller vehicles");
                return new List<object>();
            }
        }

        private async Task<List<object>> GetMonthlyRevenueAsync(int dealerId)
        {
            try
            {
                var orders = await _salesService.GetOrdersByDealerAsync(dealerId);
                var monthlyRevenue = orders
                    .Where(o => o.Status == "Delivered" && o.OrderDate.HasValue)
                    .GroupBy(o => new { 
                        Year = o.OrderDate.Value.Year, 
                        Month = o.OrderDate.Value.Month 
                    })
                    .Select(g => new
                    {
                        Month = $"{g.Key.Month:D2}/{g.Key.Year}",
                        Revenue = g.Sum(o => o.Variant?.Price ?? 0)
                    })
                    .OrderBy(x => x.Month)
                    .Take(12)
                    .ToList<object>();

                return monthlyRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly revenue");
                return new List<object>();
            }
        }
    }
}