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
            IVehicleService vehicleService
        )
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
                // Load all customers for selection
                var dealerId = GetCurrentDealerId();
                var customers = await _customerService.GetCustomersByDealerAsync(dealerId);
                ViewBag.Customers = customers;

                // If customer ID provided, preselect it
                ViewBag.PreselectedCustomerId = customerId;
                ViewBag.PreselectedVariantId = variantId;

                // Load available vehicle variants
                var variants = await _vehicleService.GetAvailableVariantsAsync();
                ViewBag.VehicleVariants = variants;

                // Create empty model for the form
                var model = new TestDriveRequestModel
                {
                    CustomerId = customerId,
                    VariantId = variantId ?? 0,
                    ScheduledDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), // Default to tomorrow
                };

                return View(model);
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
                // Validation: Check if the variant is already booked for the same date & time
                var existingTestDrives = await _customerService.GetTestDriveScheduleAsync(
                    model.ScheduledDate
                );
                var isVariantBooked = existingTestDrives.Any(td =>
                    td.VariantId == model.VariantId
                    && td.ScheduledTime == model.ScheduledTime
                    && (td.Status == "Scheduled" || td.Status == "Confirmed") // ✅ chỉ block active booking
                );

                if (isVariantBooked)
                {
                    TempData["Error"] =
                        "Xe này đã được đặt lịch lái thử vào khung giờ bạn chọn. Vui lòng chọn giờ khác.";
                    var customers = await _customerService.GetCustomersByDealerAsync(
                        GetCurrentDealerId()
                    );
                    var variants = await _vehicleService.GetAvailableVariantsAsync();
                    ViewBag.Customers = customers;
                    ViewBag.VehicleVariants = variants;
                    return View(model);
                }

                int customerId;

                // Check if customer is selected
                if (!model.CustomerId.HasValue || model.CustomerId.Value == 0)
                {
                    TempData["Error"] = "Please select a customer.";
                    return RedirectToAction(
                        nameof(ScheduleTestDrive),
                        new { variantId = model.VariantId }
                    );
                }

                customerId = model.CustomerId.Value;

                var testDrive = await _customerService.ScheduleTestDriveAsync(
                    customerId,
                    model.VariantId,
                    model.ScheduledDate,
                    model.ScheduledTime
                );

                TempData["Success"] =
                    $"Test drive scheduled successfully! Test Drive ID: {testDrive.TestDriveId}";
                return RedirectToAction(nameof(TestDrives));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error scheduling test drive: {ex.Message}";
                return RedirectToAction(
                    nameof(ScheduleTestDrive),
                    new { customerId = model.CustomerId, variantId = model.VariantId }
                );
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleVariants()
        {
            try
            {
                var variants = await _vehicleService.GetAvailableVariantsAsync();
                var variantList = variants
                    .Select(v => new
                    {
                        id = v.VariantId,
                        name = $"{v.VehicleModel.Manufacturer.Name} {v.VehicleModel.Name} {v.Version}",
                        price = v.Price,
                        color = v.Color,
                        year = v.ProductYear,
                        category = v.VehicleModel.Category,
                    })
                    .ToList();

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
                Console.WriteLine($"ConfirmTestDrive called with testDriveId: {testDriveId}");
                await _customerService.ConfirmTestDriveAsync(testDriveId);
                Console.WriteLine($"ConfirmTestDrive successful for testDriveId: {testDriveId}");
                return Json(
                    new { success = true, message = "Lịch lái thử đã được xác nhận thành công!" }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ConfirmTestDrive error: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteTestDrive(int testDriveId)
        {
            try
            {
                Console.WriteLine($"CompleteTestDrive called with testDriveId: {testDriveId}");
                await _customerService.CompleteTestDriveAsync(testDriveId);
                Console.WriteLine($"CompleteTestDrive successful for testDriveId: {testDriveId}");
                return Json(
                    new { success = true, message = "Lái thử đã được hoàn thành thành công!" }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CompleteTestDrive error: {ex.Message}");
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
                return Json(
                    new { success = true, message = "Lịch lái thử đã được hủy thành công!" }
                );
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckTimeSlotAvailability(
            string date,
            string time,
            int? variantId = null,
            int? customerId = null
        )
        {
            try
            {
                if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
                {
                    return Json(new { available = false, message = "Date and time are required" });
                }

                // Parse date and time
                var parsedDate = DateOnly.Parse(date);
                var parsedTime = TimeOnly.Parse(time);

                // Lấy danh sách test drive trong ngày
                var existingTestDrives = await _customerService.GetTestDriveScheduleAsync(
                    parsedDate
                );

                var conflictingDrive = existingTestDrives?.FirstOrDefault(td =>
                    td.ScheduledTime.HasValue
                    && td.ScheduledTime.Value == parsedTime
                    && td.VariantId == variantId
                    && td.CustomerId != customerId // Chỉ block nếu là KHÁCH KHÁC
                    && (td.Status == "Scheduled" || td.Status == "Confirmed") // Chỉ tính active booking
                );

                if (conflictingDrive != null)
                {
                    var vehicleName = conflictingDrive.Variant?.VehicleModel?.Name ?? "Unknown";
                    var variantName = conflictingDrive.Variant?.Version ?? "Unknown";
                    var customerName = conflictingDrive.Customer?.FullName ?? "Another customer";

                    return Json(
                        new
                        {
                            available = false,
                            message = $"Xe {vehicleName} - {variantName} đã được đặt bởi {customerName} lúc {parsedTime:HH:mm}. Vui lòng chọn giờ khác.",
                        }
                    );
                }

                // ✅ Nếu đến đây tức là slot trống hoặc chỉ có lịch Completed/Cancelled
                return Json(new { available = true });
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        available = false,
                        message = "Error checking time slot availability: " + ex.Message,
                    }
                );
            }
        }

        private int GetCurrentDealerId()
        {
            // Implementation to get current dealer ID from session/claims
            return HttpContext.Session.GetInt32("DealerId") ?? 1;
        }
    }
}
