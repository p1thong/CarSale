using ASM1.Repository.Models;
using ASM1.Service.Services.Interfaces;
using ASM1.WebMVC.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASM1.WebMVC.Controllers
{
    [EVMOnly]
    public class EVMController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly IDealerService _dealerService;

        public EVMController(IVehicleService vehicleService, IDealerService dealerService)
        {
            _vehicleService = vehicleService;
            _dealerService = dealerService;
        }

        // Main Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var manufacturerId = HttpContext.Session.GetString("ManufacturerId");

            ViewBag.UserRole = userRole;
            ViewBag.ManufacturerId = manufacturerId;

            try
            {
                // Load dashboard statistics
                var vehicleModels = await _vehicleService.GetAllVehicleModelsAsync();
                var vehicleVariants = await _vehicleService.GetAllVehicleVariantsAsync();
                var dealers = await _dealerService.GetAllDealersAsync();
                var manufacturers = await _vehicleService.GetAllManufacturersAsync();
                
                ViewBag.TotalVehicles = vehicleVariants.Count();
                ViewBag.TotalVehicleModels = vehicleModels.Count();
                ViewBag.ActiveDealers = dealers.Count();
                ViewBag.TotalManufacturers = manufacturers.Count();
                ViewBag.TotalContracts = 0; // TODO: Add contract counting when implemented
                
                return View();
            }
            catch (Exception)
            {
                ViewBag.TotalVehicles = 0;
                ViewBag.ActiveDealers = 0;
                ViewBag.TotalContracts = 0;
                ViewBag.TotalManufacturers = 0;
                TempData["Error"] = "Error loading dashboard data.";
                return View();
            }
        }

        // Main Flow 2: Quản lý sản phẩm & phân phối

        #region Vehicle Management

        public async Task<IActionResult> VehicleManagement()
        {
            try
            {
                // Lấy danh sách mẫu xe từ DB
                var models = await _vehicleService.GetAllVehicleModelsAsync();
                
                // Lấy danh sách phiên bản xe từ DB
                var variants = await _vehicleService.GetAllVehicleVariantsAsync();
                ViewBag.VehicleVariants = variants;
                
                return View(models);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<VehicleModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateVehicleModel()
        {
            var manufacturers = await _vehicleService.GetAllManufacturersAsync();
            ViewBag.Manufacturers = manufacturers
                .Select(m => new SelectListItem
                {
                    Value = m.ManufacturerId.ToString(),
                    Text = m.Name,
                })
                .ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicleModel(VehicleModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _vehicleService.CreateVehicleModelAsync(model);
                    TempData["Success"] = "Vehicle model created successfully!";
                    return RedirectToAction(nameof(VehicleManagement));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating vehicle model: {ex.Message}";
                }
            }

            var manufacturers = await _vehicleService.GetAllManufacturersAsync();
            ViewBag.Manufacturers = manufacturers
                .Select(m => new SelectListItem
                {
                    Value = m.ManufacturerId.ToString(),
                    Text = m.Name,
                })
                .ToList();

            return View(model);
        }

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
        public async Task<IActionResult> CreateVehicleVariant(int? modelId)
        {
            var models = await _vehicleService.GetAllVehicleModelsAsync();
            ViewBag.VehicleModels = models
                .Select(m => new SelectListItem
                {
                    Value = m.VehicleModelId.ToString(),
                    Text = $"{m.Manufacturer?.Name} {m.Name}",
                    Selected = modelId.HasValue && m.VehicleModelId == modelId.Value,
                })
                .ToList();
            ViewBag.SelectedModelId = modelId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicleVariant(VehicleVariant variant)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _vehicleService.CreateVehicleVariantAsync(variant);
                    TempData["Success"] = "Vehicle variant created successfully!";
                    return RedirectToAction(
                        nameof(VehicleVariants),
                        new { modelId = variant.VehicleModelId }
                    );
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating vehicle variant: {ex.Message}";
                }
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

        #endregion

        #region Dealer Contract Management

        public async Task<IActionResult> DealerContracts()
        {
            var dealers = await _dealerService.GetAllDealersAsync();
            return View(dealers);
        }

        [HttpGet]
        public async Task<IActionResult> CreateDealerContract(int dealerId)
        {
            var dealer = await _dealerService.GetDealerByIdAsync(dealerId);
            var manufacturers = await _vehicleService.GetAllManufacturersAsync();

            ViewBag.DealerName = dealer?.FullName;
            ViewBag.DealerId = dealerId;
            ViewBag.Manufacturers = manufacturers
                .Select(m => new SelectListItem
                {
                    Value = m.ManufacturerId.ToString(),
                    Text = m.Name,
                })
                .ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDealerContract(DealerContract contract)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _dealerService.CreateDealerContractAsync(contract);
                    TempData["Success"] = "Dealer contract created successfully!";
                    return RedirectToAction(nameof(DealerContracts));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating dealer contract: {ex.Message}";
                }
            }

            var dealer = await _dealerService.GetDealerByIdAsync(contract.DealerId);
            var manufacturers = await _vehicleService.GetAllManufacturersAsync();

            ViewBag.DealerName = dealer?.FullName;
            ViewBag.DealerId = contract.DealerId;
            ViewBag.Manufacturers = manufacturers
                .Select(m => new SelectListItem
                {
                    Value = m.ManufacturerId.ToString(),
                    Text = m.Name,
                })
                .ToList();

            return View(contract);
        }

        [HttpGet]
        public async Task<IActionResult> ViewDealerContracts(int dealerId)
        {
            var contracts = await _dealerService.GetDealerContractsAsync(dealerId);
            var dealer = await _dealerService.GetDealerByIdAsync(dealerId);
            ViewBag.DealerName = dealer?.FullName;
            ViewBag.DealerId = dealerId;
            return View(contracts);
        }

        #endregion

        #region Distribution Monitoring

        public async Task<IActionResult> DistributionMonitoring()
        {
            var dealers = await _dealerService.GetAllDealersAsync();
            return View(dealers);
        }

        [HttpGet]
        public async Task<IActionResult> DealerSalesReport(int dealerId)
        {
            // TODO: Implement sales report logic
            var dealer = await _dealerService.GetDealerByIdAsync(dealerId);
            ViewBag.DealerName = dealer?.FullName;
            ViewBag.DealerId = dealerId;
            return View();
        }

        #endregion

        #region Promotion Management

        public IActionResult Promotions()
        {
            // TODO: Implement promotion management
            return View();
        }

        [HttpGet]
        public IActionResult CreatePromotion()
        {
            // TODO: Implement create promotion
            return View();
        }

        [HttpPost]
        public IActionResult CreatePromotion(Promotion promotion)
        {
            // TODO: Implement create promotion logic
            return View(promotion);
        }

        #endregion
    }
}
