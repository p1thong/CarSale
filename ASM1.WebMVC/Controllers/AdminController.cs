using ASM1.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASM1.WebMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IVehicleService _vehicleService;
        private readonly IDealerService _dealerService;

        public AdminController(
            ILogger<AdminController> logger,
            IVehicleService vehicleService,
            IDealerService dealerService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            _dealerService = dealerService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // Load dashboard statistics from database
                var manufacturers = await _vehicleService.GetAllManufacturersAsync();
                var dealers = await _dealerService.GetAllDealersAsync();
                var vehicleModels = await _vehicleService.GetAllVehicleModelsAsync();
                
                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                ViewBag.TotalManufacturers = manufacturers.Count();
                ViewBag.TotalDealers = dealers.Count();
                ViewBag.TotalVehicleModels = vehicleModels.Count();
                ViewBag.TotalUsers = 0; // TODO: Add user count when IUserService is available

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard data");
                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                ViewBag.TotalManufacturers = 0;
                ViewBag.TotalDealers = 0;
                ViewBag.TotalVehicleModels = 0;
                ViewBag.TotalUsers = 0;
                TempData["Error"] = "Error loading dashboard statistics.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult ManageUsers()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ManageManufacturers()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            var manufacturers = await _vehicleService.GetAllManufacturersAsync();
            return View(manufacturers);
        }

        [HttpGet]
        public async Task<IActionResult> ManageDealers()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            var dealers = await _dealerService.GetAllDealersAsync();
            return View(dealers);
        }

        [HttpGet]
        public IActionResult SystemReports()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Settings()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }
    }
}