using ASM1.Repository.Models;
using ASM1.Service.Services.Interfaces;
using ASM1.WebMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace ASM1.WebMVC.Controllers
{
    public class CustomerServiceController : Controller
    {
        private readonly ICustomerRelationshipService _customerService;
        private readonly IVehicleService _vehicleService;

        public CustomerServiceController(
            ICustomerRelationshipService customerService,
            IVehicleService vehicleService)
        {
            _customerService = customerService;
            _vehicleService = vehicleService;
        }

        // Test Drive Management
        public async Task<IActionResult> TestDrives(DateTime? date = null)
        {
            try
            {
                // Nếu có tham số date, hiển thị theo ngày đó
                if (date.HasValue)
                {
                    var selectedDate = DateOnly.FromDateTime(date.Value);
                    var testDrives = await _customerService.GetTestDriveScheduleAsync(selectedDate);
                    
                    if (testDrives == null || !testDrives.Any())
                    {
                        ViewBag.Message = "Không có lịch lái thử nào cho ngày đã chọn.";
                    }
                    
                    ViewBag.SelectedDate = selectedDate;
                    return View(testDrives);
                }
                else
                {
                    // Hiển thị tất cả test drives gần đây (không filter theo ngày)
                    var allTestDrives = await _customerService.GetAllTestDrivesAsync();
                    
                    if (allTestDrives == null || !allTestDrives.Any())
                    {
                        ViewBag.Message = "Chưa có lịch lái thử nào.";
                    }
                    
                    ViewBag.SelectedDate = DateOnly.FromDateTime(DateTime.Today);
                    return View(allTestDrives);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải danh sách lịch lái thử: {ex.Message}";
                return View(new List<TestDrive>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ScheduleTestDrive(int? customerId, int? variantId)
        {
            try
            {
                // If no customer ID provided, create a simple form for guest users
                if (!customerId.HasValue)
                {
                    ViewBag.IsGuestUser = true;
                    ViewBag.PreselectedVariantId = variantId;
                }
                else
                {
                    ViewBag.CustomerId = customerId;
                    ViewBag.IsGuestUser = false;
                }
                
                // Load available vehicle variants
                var variants = await _vehicleService.GetAvailableVariantsAsync();
                ViewBag.VehicleVariants = variants;
                
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading test drive form: {ex.Message}";
                return RedirectToAction(nameof(TestDrives));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleTestDrive(TestDriveRequestModel model)
        {
            try
            {
                int customerId;
                
                // If no existing customer ID, create a new customer from guest info
                if (!model.CustomerId.HasValue || model.CustomerId.Value == 0)
                {
                    if (string.IsNullOrEmpty(model.GuestName) || string.IsNullOrEmpty(model.GuestEmail) || string.IsNullOrEmpty(model.GuestPhone))
                    {
                        TempData["Error"] = "Guest information is required when scheduling as a guest.";
                        return RedirectToAction(nameof(ScheduleTestDrive), new { variantId = model.VariantId });
                    }

                    // Create new customer
                    var dealerId = 1; // Default dealer ID, you might want to get this from session or config
                    var newCustomer = new Customer
                    {
                        DealerId = dealerId,
                        FullName = model.GuestName,
                        Email = model.GuestEmail,
                        Phone = model.GuestPhone,
                        Birthday = model.GuestBirthday
                    };

                    var createdCustomer = await _customerService.CreateCustomerAsync(newCustomer);
                    customerId = createdCustomer.CustomerId;
                }
                else
                {
                    customerId = model.CustomerId.Value;
                }

                var testDrive = await _customerService.ScheduleTestDriveAsync(
                    customerId,
                    model.VariantId,
                    model.ScheduledDate
                );
                
                TempData["Success"] = $"Test drive scheduled successfully! Test Drive ID: {testDrive.TestDriveId}";
                return RedirectToAction(nameof(TestDrives));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error scheduling test drive: {ex.Message}";
                return RedirectToAction(nameof(ScheduleTestDrive), new { customerId = model.CustomerId, variantId = model.VariantId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleVariants()
        {
            try
            {
                var variants = await _vehicleService.GetAvailableVariantsAsync();
                var variantList = variants.Select(v => new
                {
                    id = v.VariantId,
                    name = $"{v.VehicleModel.Manufacturer.Name} {v.VehicleModel.Name} {v.Version}",
                    price = v.Price,
                    color = v.Color,
                    year = v.ProductYear,
                    category = v.VehicleModel.Category
                }).ToList();
                
                return Json(variantList);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmTestDrive(int testDriveId)
        {
            try
            {
                await _customerService.ConfirmTestDriveAsync(testDriveId);
                return Json(new { success = true, message = "Test drive confirmed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteTestDrive(int testDriveId)
        {
            try
            {
                await _customerService.CompleteTestDriveAsync(testDriveId);
                return Json(new { success = true, message = "Test drive completed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Customer Profile & History
        public async Task<IActionResult> CustomerProfile(int customerId)
        {
            try
            {
                var profile = await _customerService.GetCustomerProfileAsync(customerId);
                if (profile == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction("Customers", "Sales");
                }
                return View(profile);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading customer profile: {ex.Message}";
                return RedirectToAction("Customers", "Sales");
            }
        }

        public async Task<IActionResult> CustomerOrderHistory(int customerId)
        {
            try
            {
                var orders = await _customerService.GetCustomerOrderHistoryAsync(customerId);
                ViewBag.CustomerId = customerId;
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading order history: {ex.Message}";
                return View(new List<Order>());
            }
        }

        // Feedback Management
        public async Task<IActionResult> Feedbacks()
        {
            try
            {
                var feedbacks = await _customerService.GetAllFeedbacksAsync();
                return View(feedbacks);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading feedbacks: {ex.Message}";
                return View(new List<Feedback>());
            }
        }

        [HttpGet]
        public IActionResult CreateFeedback(int customerId)
        {
            try
            {
                ViewBag.CustomerId = customerId;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading feedback form: {ex.Message}";
                return RedirectToAction(nameof(Feedbacks));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback(int customerId, string content, int rating)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    TempData["Error"] = "Feedback content is required.";
                    return RedirectToAction(nameof(CreateFeedback), new { customerId });
                }

                if (rating < 1 || rating > 5)
                {
                    TempData["Error"] = "Rating must be between 1 and 5.";
                    return RedirectToAction(nameof(CreateFeedback), new { customerId });
                }

                await _customerService.CreateFeedbackAsync(customerId, content, rating);
                TempData["Success"] = "Feedback created successfully!";
                return RedirectToAction(nameof(Feedbacks));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating feedback: {ex.Message}";
                return RedirectToAction(nameof(CreateFeedback), new { customerId });
            }
        }

        // Reports
        public async Task<IActionResult> CustomerCareReport()
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                var fromDate = DateTime.Today.AddMonths(-1);
                var toDate = DateTime.Today;

                var report = await _customerService.GenerateCustomerCareReportAsync(
                    dealerId,
                    fromDate,
                    toDate
                );
                return View(report);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating customer care report: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateCustomReport(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                var report = await _customerService.GenerateCustomerCareReportAsync(
                    dealerId,
                    fromDate,
                    toDate
                );
                return PartialView("_CustomerCareReportPartial", report);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Additional Customer Service Methods
        public async Task<IActionResult> CustomerPromotions(int customerId)
        {
            try
            {
                var promotions = await _customerService.GetCustomerEligiblePromotionsAsync(
                    customerId
                );
                ViewBag.CustomerId = customerId;
                return View(promotions);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading promotions: {ex.Message}";
                return View(new List<Promotion>());
            }
        }

        public async Task<IActionResult> CustomerOutstandingBalance(int customerId)
        {
            try
            {
                var balance = await _customerService.GetCustomerOutstandingBalanceAsync(customerId);
                var paymentHistory = await _customerService.GetCustomerPaymentHistoryAsync(
                    customerId
                );

                ViewBag.CustomerId = customerId;
                ViewBag.OutstandingBalance = balance;

                return View(paymentHistory);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading payment information: {ex.Message}";
                return View(new List<Payment>());
            }
        }

        public async Task<IActionResult> TestDriveDetails(int id)
        {
            try
            {
                // Cần thêm method GetTestDriveByIdAsync vào service
                var testDrive = await _customerService.GetTestDriveByIdAsync(id);
                
                if (testDrive == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin lịch lái thử.";
                    return RedirectToAction(nameof(TestDrives));
                }

                return View(testDrive);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải thông tin lịch lái thử: {ex.Message}";
                return RedirectToAction(nameof(TestDrives));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelTestDrive(int testDriveId)
        {
            try
            {
                await _customerService.UpdateTestDriveStatusAsync(testDriveId, "Cancelled");
                return Json(new { success = true, message = "Lịch lái thử đã được hủy thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetCurrentDealerId()
        {
            // Implementation to get current dealer ID from session/claims
            return HttpContext.Session.GetInt32("DealerId") ?? 1;
        }
    }
}
