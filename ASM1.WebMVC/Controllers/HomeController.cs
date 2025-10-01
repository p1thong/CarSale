using System.Diagnostics;
using ASM1.WebMVC.Models;
using ASM1.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASM1.WebMVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVehicleService _vehicleService;

        public HomeController(ILogger<HomeController> logger, IVehicleService vehicleService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy danh sách tất cả vehicle variants với thông tin liên quan
                var vehicles = await _vehicleService.GetAllVehicleVariantsAsync();
                return View(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicles for homepage");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách xe. Vui lòng thử lại sau.";
                return View(new List<ASM1.Repository.Models.VehicleVariant>());
            }
        }

        public async Task<IActionResult> VehicleDetail(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleVariantByIdAsync(id);
                if (vehicle == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin xe.";
                    return RedirectToAction("Index");
                }
                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicle detail for ID: {VehicleId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin xe. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                }
            );
        }
    }
}
