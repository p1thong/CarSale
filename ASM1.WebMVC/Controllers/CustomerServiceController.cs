using ASM1.Repository.Models;
using ASM1.Service.Services.Interfaces;
using ASM1.WebMVC.Models;
using ASM1.WebMVC.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace ASM1.WebMVC.Controllers
{
    public class CustomerServiceController : BaseController
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

        // Test Drive Management - Dealer Staff and Customers (quản lý lịch lái thử)
        [CustomerAndDealer]
        public async Task<IActionResult> TestDrives(DateTime? date = null)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                
                // Nếu là Customer, chuyển hướng đến MyTestDrives
                if (string.Equals(userRole, "Customer", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction(nameof(MyTestDrives));
                }

                // Nếu có tham số date, hiển thị theo ngày đó (chỉ dành cho Dealer Staff)
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
                    // Hiển thị tất cả test drives gần đây (không filter theo ngày) - chỉ dành cho Dealer Staff
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

        // My Test Drives - Customer Only (xem lịch lái thử của chính họ)
        [CustomerOnly]
        public async Task<IActionResult> MyTestDrives()
        {
            try
            {
                var currentCustomer = await GetCurrentCustomerAsync();
                if (currentCustomer == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin khách hàng. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Login", "Auth");
                }

                // Lấy tất cả test drives của customer hiện tại
                var customerTestDrives = await _customerService.GetCustomerTestDrivesAsync(currentCustomer.CustomerId);

                if (customerTestDrives == null || !customerTestDrives.Any())
                {
                    ViewBag.Message = "Bạn chưa có lịch lái thử nào.";
                }

                ViewBag.CustomerName = currentCustomer.FullName;
                return View(customerTestDrives);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải lịch lái thử: {ex.Message}";
                return View(new List<TestDrive>());
            }
        }

        [HttpGet]
        // Customer đặt lịch lái thử xe - Customer can schedule test drives
        [CustomerAndDealer]
        public async Task<IActionResult> ScheduleTestDrive(int? customerId, int? variantId)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                
                // Nếu là Customer đăng nhập, tự động lấy thông tin Customer hiện tại
                if (userRole?.Equals("customer", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var currentCustomer = await GetCurrentCustomerAsync();
                    if (currentCustomer != null)
                    {
                        // Chỉ load customer hiện tại, không cho chọn customer khác
                        ViewBag.Customers = new List<Customer> { currentCustomer };
                        ViewBag.PreselectedCustomerId = currentCustomer.CustomerId;
                        ViewBag.IsCurrentCustomer = true; // Flag để UI biết đây là customer hiện tại
                    }
                }
                else
                {
                    // Nếu là Dealer Staff, cho phép chọn customer
                    var dealerId = GetCurrentDealerId();
                    var customers = await _customerService.GetCustomersByDealerAsync(dealerId);
                    ViewBag.Customers = customers;
                    ViewBag.PreselectedCustomerId = customerId;
                    ViewBag.IsCurrentCustomer = false;
                }

                ViewBag.PreselectedVariantId = variantId;

                // Load available vehicle variants
                var variants = await _vehicleService.GetAvailableVariantsAsync();
                ViewBag.VehicleVariants = variants;

                // Create empty model for the form
                var model = new TestDriveRequestModel
                {
                    CustomerId = ViewBag.PreselectedCustomerId,
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

        [CustomerAndDealer]
        [HttpPost]
        public async Task<IActionResult> ScheduleTestDrive(TestDriveRequestModel model)
        {
            try
            {
                // Validation: Check if the variant is already booked for the same date & time
                var existingTestDrives = await _customerService.GetTestDriveScheduleAsync(
                    model.ScheduledDate
                );

                // ❌ Rule 1: Khách khác đã giữ variant này cùng giờ
                var isVariantBookedByOther = existingTestDrives.Any(td =>
                    td.VariantId == model.VariantId
                    && td.ScheduledTime == model.ScheduledTime
                    && td.CustomerId != model.CustomerId
                    && (td.Status == "Scheduled" || td.Status == "Confirmed")
                );

                if (isVariantBookedByOther)
                {
                    TempData["Error"] =
                        "Xe này đã được khách hàng khác đặt vào khung giờ bạn chọn. Vui lòng chọn giờ khác.";
                    var customers = await _customerService.GetCustomersByDealerAsync(
                        GetCurrentDealerId()
                    );
                    var variants = await _vehicleService.GetAvailableVariantsAsync();
                    ViewBag.Customers = customers;
                    ViewBag.VehicleVariants = variants;
                    return View(model);
                }

                // ❌ Rule 2: Chính khách này đã giữ một variant khác cùng giờ
                var hasOtherVariantSameTime = existingTestDrives.Any(td =>
                    td.CustomerId == model.CustomerId
                    && td.VariantId != model.VariantId
                    && td.ScheduledTime == model.ScheduledTime
                    && (td.Status == "Scheduled" || td.Status == "Confirmed")
                );

                if (hasOtherVariantSameTime)
                {
                    TempData["Error"] =
                        "Bạn đã có lịch lái thử một xe khác trong cùng khung giờ. Vui lòng chọn giờ khác.";
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

        // Dealer Staff xác nhận lịch lái thử - Dealer roles can confirm
        [DealerOnly]
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

        // Dealer Staff hoàn thành lịch lái thử - Dealer roles can complete
        [DealerOnly]
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
        // View all feedbacks - All dealer roles can view
        [DealerOnly]
        public async Task<IActionResult> Feedbacks()
        {
            try
            {
                var currentDealerId = GetCurrentDealerId();
                var feedbacks = await _customerService.GetFeedbacksByDealerAsync(currentDealerId);
                return View(feedbacks);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading feedbacks: {ex.Message}";
                return View(new List<Feedback>());
            }
        }

        // Khách hàng gửi phản hồi - Customer feedback creation
        [CustomerAndDealer]
        [HttpGet]
        public async Task<IActionResult> CreateFeedback(int customerId, int? testDriveId = null)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(TestDrives));
                }
                
                ViewBag.CustomerId = customerId;
                ViewBag.CustomerName = customer.FullName;
                ViewBag.TestDriveId = testDriveId;
                
                // Nếu có testDriveId, lấy thông tin test drive để hiển thị context
                if (testDriveId.HasValue)
                {
                    try
                    {
                        var testDrive = await _customerService.GetTestDriveByIdAsync(testDriveId.Value);
                        if (testDrive != null)
                        {
                            ViewBag.TestDriveInfo = $"Lái thử {testDrive.Variant?.VehicleModel?.ModelName} vào {testDrive.ScheduledDate:dd/MM/yyyy}";
                        }
                    }
                    catch
                    {
                        // Không có vấn đề gì nếu không lấy được thông tin test drive
                    }
                }
                
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading feedback form: {ex.Message}";
                return RedirectToAction(nameof(Feedbacks));
            }
        }

        [CustomerAndDealer]
        [HttpPost]
        public async Task<IActionResult> CreateFeedback(int customerId, string content, int rating, int? testDriveId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    TempData["Error"] = "Feedback content is required.";
                    return RedirectToAction(nameof(CreateFeedback), new { customerId, testDriveId });
                }

                if (rating < 1 || rating > 5)
                {
                    TempData["Error"] = "Rating must be between 1 and 5.";
                    return RedirectToAction(nameof(CreateFeedback), new { customerId, testDriveId });
                }

                await _customerService.CreateFeedbackAsync(customerId, content, rating);
                TempData["Success"] = "Feedback created successfully!";
                return RedirectToAction(nameof(Feedbacks));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating feedback: {ex.Message}";
                return RedirectToAction(nameof(CreateFeedback), new { customerId, testDriveId });
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

        // View test drive details - Both Customer and Dealer can view
        [CustomerAndDealer]
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

        // Cancel test drive - Both Customer and Dealer can cancel
        [CustomerAndDealer]
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

                var parsedDate = DateOnly.Parse(date);
                var parsedTime = TimeOnly.Parse(time);

                var existingTestDrives = await _customerService.GetTestDriveScheduleAsync(
                    parsedDate
                );

                // ❌ Block nếu KHÁCH KHÁC đã đặt cùng xe tại giờ đó (Scheduled/Confirmed)
                var conflictingDrive = existingTestDrives?.FirstOrDefault(td =>
                    td.ScheduledTime.HasValue
                    && td.ScheduledTime.Value == parsedTime
                    && td.VariantId == variantId
                    && td.CustomerId != customerId
                    && (td.Status == "Scheduled" || td.Status == "Confirmed")
                );

                if (conflictingDrive != null)
                {
                    return Json(
                        new
                        {
                            available = false,
                            message = $"Xe {conflictingDrive.Variant?.VehicleModel?.Name} - {conflictingDrive.Variant?.Version} đã được đặt bởi {conflictingDrive.Customer?.FullName ?? "người khác"} lúc {parsedTime:HH:mm}.",
                        }
                    );
                }

                // ❌ Block nếu CHÍNH KHÁCH HÀNG đã đặt 1 xe khác cùng giờ
                var customerSameTimeDrive = existingTestDrives?.FirstOrDefault(td =>
                    td.ScheduledTime.HasValue
                    && td.ScheduledTime.Value == parsedTime
                    && td.CustomerId == customerId
                    && td.VariantId != variantId
                    && (td.Status == "Scheduled" || td.Status == "Confirmed")
                );

                if (customerSameTimeDrive != null)
                {
                    return Json(
                        new
                        {
                            available = false,
                            message = $"Bạn đã có lịch lái thử xe {customerSameTimeDrive.Variant?.VehicleModel?.Name} - {customerSameTimeDrive.Variant?.Version} vào lúc {parsedTime:HH:mm}. Một khách chỉ có thể lái thử 1 xe trong 1 khung giờ.",
                        }
                    );
                }

                // ✅ Nếu không conflict -> cho phép
                return Json(new { available = true });
            }
            catch (Exception ex)
            {
                return Json(new { available = false, message = "Error: " + ex.Message });
            }
        }

        private int GetCurrentDealerId()
        {
            // Lấy dealerId từ session hoặc từ user hiện tại
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (userRole?.Equals("customer", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Nếu là customer, lấy dealerId từ User table (đã được set khi đăng ký)
                var userIdString = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out var userId))
                {
                    // Tạm thời return dealerId = 1, có thể cần query database để lấy chính xác
                    return 1; // TODO: Query User table để lấy dealerId chính xác
                }
            }
            
            // Cho Dealer Staff hoặc fallback
            return HttpContext.Session.GetInt32("DealerId") ?? 1;
        }

        private async Task<Customer?> GetCurrentCustomerAsync()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return null;
                }

                // Tìm customer có email trùng với user hiện tại
                var dealerId = GetCurrentDealerId();
                var customers = await _customerService.GetCustomersByDealerAsync(dealerId);
                return customers.FirstOrDefault(c => c.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return null;
            }
        }

        // Submit Feedback - Customer only can submit feedback for completed test drives
        [CustomerOnly]
        public async Task<IActionResult> SubmitFeedback(int testDriveId)
        {
            try
            {
                // Get test drive details
                var testDrive = await _customerService.GetTestDriveByIdAsync(testDriveId);
                if (testDrive == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin lịch lái thử.";
                    return RedirectToAction(nameof(MyTestDrives));
                }

                // Check if test drive is completed
                if (testDrive.Status != "Completed")
                {
                    TempData["ErrorMessage"] = "Chỉ có thể gửi phản hồi cho lịch lái thử đã hoàn thành.";
                    return RedirectToAction(nameof(MyTestDrives));
                }

                // Verify this test drive belongs to current customer
                var currentCustomer = await GetCurrentCustomerAsync();
                if (currentCustomer == null || testDrive.CustomerId != currentCustomer.CustomerId)
                {
                    TempData["ErrorMessage"] = "Bạn chỉ có thể gửi phản hồi cho lịch lái thử của chính mình.";
                    return RedirectToAction(nameof(MyTestDrives));
                }

                // Create feedback model
                var feedback = new Feedback
                {
                    CustomerId = currentCustomer.CustomerId,
                    FeedbackDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                ViewBag.TestDrive = testDrive;
                return View(feedback);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải trang phản hồi: {ex.Message}";
                return RedirectToAction(nameof(MyTestDrives));
            }
        }

        [CustomerOnly]
        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(Feedback feedback, int testDriveId)
        {
            try
            {
                // Debug logging
                Console.WriteLine($"SubmitFeedback POST called with testDriveId: {testDriveId}");
                Console.WriteLine($"Feedback Rating: {feedback.Rating}, Content: {feedback.Content}");

                // Verify test drive
                var testDrive = await _customerService.GetTestDriveByIdAsync(testDriveId);
                if (testDrive == null || testDrive.Status != "Completed")
                {
                    TempData["ErrorMessage"] = "Không thể gửi phản hồi cho lịch lái thử này.";
                    return RedirectToAction(nameof(MyTestDrives));
                }

                // Verify customer
                var currentCustomer = await GetCurrentCustomerAsync();
                if (currentCustomer == null || testDrive.CustomerId != currentCustomer.CustomerId)
                {
                    TempData["ErrorMessage"] = "Bạn chỉ có thể gửi phản hồi cho lịch lái thử của chính mình.";
                    return RedirectToAction(nameof(MyTestDrives));
                }

                // Clear ModelState errors for navigation properties
                ModelState.Remove("Customer");
                
                // Set feedback data
                feedback.CustomerId = currentCustomer.CustomerId;
                feedback.FeedbackDate = DateTime.Now;
                feedback.CreatedAt = DateTime.Now;

                // Validate rating
                if (feedback.Rating == null || feedback.Rating < 1 || feedback.Rating > 5)
                {
                    ModelState.AddModelError("Rating", "Điểm đánh giá phải từ 1 đến 5 sao.");
                }

                // Validate content
                if (string.IsNullOrWhiteSpace(feedback.Content))
                {
                    ModelState.AddModelError("Content", "Vui lòng nhập nội dung phản hồi.");
                }

                Console.WriteLine($"ModelState IsValid after validation: {ModelState.IsValid}");
                if (!ModelState.IsValid)
                {
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"ModelState Error: {error.Key} - {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.TestDrive = testDrive;
                    return View(feedback);
                }

                // Submit feedback
                await _customerService.CreateFeedbackAsync(
                    feedback.CustomerId, 
                    feedback.Content ?? string.Empty, 
                    feedback.Rating ?? 5
                );

                TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi phản hồi! Ý kiến của bạn rất quan trọng với chúng tôi.";
                return RedirectToAction(nameof(MyTestDrives));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SubmitFeedback: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi gửi phản hồi: {ex.Message}";
                ViewBag.TestDrive = await _customerService.GetTestDriveByIdAsync(testDriveId);
                return View(feedback);
            }
        }
    }
}
