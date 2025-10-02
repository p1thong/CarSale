using ASM1.Repository.Models;
using ASM1.Service.Services.Interfaces;
using ASM1.WebMVC.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASM1.WebMVC.Controllers
{
    [DealerOnly]
    public class DealerController : BaseController
    {
        private readonly ICustomerRelationshipService _customerService;
        private readonly ISalesService _salesService;
        private readonly IVehicleService _vehicleService;

        public DealerController(
            ICustomerRelationshipService customerService, 
            ISalesService salesService,
            IVehicleService vehicleService)
        {
            _customerService = customerService;
            _salesService = salesService;
            _vehicleService = vehicleService;
        }

        // Main Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var dealerIdStr = HttpContext.Session.GetString("DealerId");
            
            ViewBag.UserRole = userRole;
            ViewBag.DealerId = dealerIdStr;

            try
            {
                var dealerId = GetCurrentDealerId();
                
                // Load dashboard statistics
                var customers = await _customerService.GetCustomersByDealerAsync(dealerId);
                var quotations = await _salesService.GetQuotationsByDealerAsync(dealerId);
                var orders = await _salesService.GetOrdersByDealerAsync(dealerId);
                
                ViewBag.TotalCustomers = customers.Count();
                ViewBag.TotalTestDrives = 0; // TODO: Add method to count test drives
                ViewBag.TotalQuotations = quotations.Count();
                ViewBag.ActiveOrders = orders.Count(o => o.Status == "Pending" || o.Status == "Processing");
                
                return View();
            }
            catch (Exception)
            {
                ViewBag.TotalCustomers = 0;
                ViewBag.TotalTestDrives = 0;
                ViewBag.TotalQuotations = 0;
                ViewBag.ActiveOrders = 0;
                TempData["Error"] = "Error loading dashboard data.";
                return View();
            }
        }

        // Main Flow 1: Quy trình bán hàng

        #region Customer Management

        public async Task<IActionResult> Customers()
        {
            var dealerId = GetCurrentDealerId();
            var customers = await _customerService.GetCustomersByDealerAsync(dealerId);
            return View(customers);
        }

        [HttpGet]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer(Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    customer.DealerId = GetCurrentDealerId();
                    await _customerService.CreateCustomerAsync(customer);
                    TempData["Success"] = "Customer created successfully!";
                    return RedirectToAction(nameof(Customers));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating customer: {ex.Message}";
                }
            }
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> EditCustomer(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null || customer.DealerId != GetCurrentDealerId())
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> EditCustomer(Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    customer.DealerId = GetCurrentDealerId();
                    await _customerService.UpdateCustomerAsync(customer);
                    TempData["Success"] = "Customer updated successfully!";
                    return RedirectToAction(nameof(Customers));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error updating customer: {ex.Message}";
                }
            }
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerProfile(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null || customer.DealerId != GetCurrentDealerId())
            {
                return NotFound();
            }
            
            // Get customer's orders, quotations, test drives, feedbacks
            ViewBag.CustomerId = id;
            return View(customer);
        }

        #endregion

        #region Quotation Management

        public async Task<IActionResult> Quotations()
        {
            var dealerId = GetCurrentDealerId();
            var quotations = await _salesService.GetQuotationsByDealerAsync(dealerId);
            return View(quotations);
        }

        [HttpGet]
        public async Task<IActionResult> CreateQuotation(int? customerId)
        {
            var dealerId = GetCurrentDealerId();
            var customers = await _customerService.GetCustomersByDealerAsync(dealerId);
            var variants = await _vehicleService.GetAllVehicleVariantsAsync();

            ViewBag.Customers = customers.Select(c => new SelectListItem
            {
                Value = c.CustomerId.ToString(),
                Text = $"{c.FullName} - {c.Email}",
                Selected = customerId.HasValue && c.CustomerId == customerId.Value
            }).ToList();

            ViewBag.VehicleVariants = variants.Select(v => new SelectListItem
            {
                Value = v.VariantId.ToString(),
                Text = $"{v.VehicleModel?.Manufacturer?.Name} {v.VehicleModel?.Name} {v.Version} - {v.Color}"
            }).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuotation(Quotation quotation)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    quotation.DealerId = GetCurrentDealerId();
                    quotation.CreatedAt = DateTime.Now;
                    quotation.Status = "Pending";
                    
                    await _salesService.CreateQuotationAsync(quotation);
                    TempData["Success"] = "Quotation created successfully!";
                    return RedirectToAction(nameof(Quotations));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating quotation: {ex.Message}";
                }
            }

            await LoadQuotationViewData();
            return View(quotation);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveQuotation(int id)
        {
            try
            {
                var quotation = await _salesService.GetQuotationByIdAsync(id);
                if (quotation != null && quotation.DealerId == GetCurrentDealerId())
                {
                    await _salesService.ApproveQuotationAsync(id);
                    TempData["Success"] = "Quotation approved successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error approving quotation: {ex.Message}";
            }
            return RedirectToAction(nameof(Quotations));
        }

        #endregion

        #region Order Management

        public async Task<IActionResult> Orders()
        {
            var dealerId = GetCurrentDealerId();
            var orders = await _salesService.GetOrdersByDealerAsync(dealerId);
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrder(int? quotationId)
        {
            var dealerId = GetCurrentDealerId();
            
            if (quotationId.HasValue)
            {
                var quotation = await _salesService.GetQuotationByIdAsync(quotationId.Value);
                if (quotation != null && quotation.DealerId == dealerId)
                {
                    var order = new Order
                    {
                        CustomerId = quotation.CustomerId,
                        VariantId = quotation.VariantId,
                        DealerId = dealerId
                    };
                    ViewBag.QuotationId = quotationId.Value;
                    return View(order);
                }
            }

            var customers = await _customerService.GetCustomersByDealerAsync(dealerId);
            var variants = await _vehicleService.GetAllVehicleVariantsAsync();

            ViewBag.Customers = customers.Select(c => new SelectListItem
            {
                Value = c.CustomerId.ToString(),
                Text = $"{c.FullName} - {c.Email}"
            }).ToList();

            ViewBag.VehicleVariants = variants.Select(v => new SelectListItem
            {
                Value = v.VariantId.ToString(),
                Text = $"{v.VehicleModel?.Manufacturer?.Name} {v.VehicleModel?.Name} {v.Version}"
            }).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order, int? quotationId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    order.DealerId = GetCurrentDealerId();
                    order.OrderDate = DateOnly.FromDateTime(DateTime.Now);
                    order.Status = "Pending";
                    
                    await _salesService.CreateOrderAsync(order);
                    
                    // Update quotation status if created from quotation
                    if (quotationId.HasValue)
                    {
                        await _salesService.UpdateQuotationStatusAsync(quotationId.Value, "Accepted");
                    }
                    
                    TempData["Success"] = "Order created successfully!";
                    return RedirectToAction(nameof(Orders));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating order: {ex.Message}";
                }
            }

            await LoadOrderViewData();
            return View(order);
        }

        #endregion

        #region Test Drive Management

        public async Task<IActionResult> TestDrives()
        {
            var dealerId = GetCurrentDealerId();
            var testDrives = await _customerService.GetTestDrivesByDealerAsync(dealerId);
            return View(testDrives);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTestDrive(int? customerId)
        {
            var dealerId = GetCurrentDealerId();
            var customers = await _customerService.GetCustomersByDealerAsync(dealerId);
            var variants = await _vehicleService.GetAllVehicleVariantsAsync();

            ViewBag.Customers = customers.Select(c => new SelectListItem
            {
                Value = c.CustomerId.ToString(),
                Text = $"{c.FullName} - {c.Phone}",
                Selected = customerId.HasValue && c.CustomerId == customerId.Value
            }).ToList();

            ViewBag.VehicleVariants = variants.Select(v => new SelectListItem
            {
                Value = v.VariantId.ToString(),
                Text = $"{v.VehicleModel?.Manufacturer?.Name} {v.VehicleModel?.Name} {v.Version}"
            }).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestDrive(TestDrive testDrive)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    testDrive.Status = "Scheduled";
                    await _customerService.CreateTestDriveAsync(testDrive);
                    TempData["Success"] = "Test drive scheduled successfully!";
                    return RedirectToAction(nameof(TestDrives));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error scheduling test drive: {ex.Message}";
                }
            }

            await LoadTestDriveViewData();
            return View(testDrive);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmTestDrive(int id)
        {
            try
            {
                await _customerService.UpdateTestDriveStatusAsync(id, "Confirmed");
                TempData["Success"] = "Test drive confirmed successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error confirming test drive: {ex.Message}";
            }
            return RedirectToAction(nameof(TestDrives));
        }

        #endregion

        #region Feedback Management

        public async Task<IActionResult> Feedbacks()
        {
            var dealerId = GetCurrentDealerId();
            var feedbacks = await _customerService.GetFeedbacksByDealerAsync(dealerId);
            return View(feedbacks);
        }

        #endregion

        #region Helper Methods

        private int GetCurrentDealerId()
        {
            var dealerIdStr = HttpContext.Session.GetString("DealerId");
            return int.TryParse(dealerIdStr, out int dealerId) ? dealerId : 0;
        }

        private async Task LoadQuotationViewData()
        {
            var dealerId = GetCurrentDealerId();
            var customers = await _customerService.GetCustomersByDealerAsync(dealerId);
            var variants = await _vehicleService.GetAllVehicleVariantsAsync();

            ViewBag.Customers = customers.Select(c => new SelectListItem
            {
                Value = c.CustomerId.ToString(),
                Text = $"{c.FullName} - {c.Email}"
            }).ToList();

            ViewBag.VehicleVariants = variants.Select(v => new SelectListItem
            {
                Value = v.VariantId.ToString(),
                Text = $"{v.VehicleModel?.Manufacturer?.Name} {v.VehicleModel?.Name} {v.Version}"
            }).ToList();
        }

        private async Task LoadOrderViewData()
        {
            var dealerId = GetCurrentDealerId();
            var customers = await _customerService.GetCustomersByDealerAsync(dealerId);
            var variants = await _vehicleService.GetAllVehicleVariantsAsync();

            ViewBag.Customers = customers.Select(c => new SelectListItem
            {
                Value = c.CustomerId.ToString(),
                Text = $"{c.FullName} - {c.Email}"
            }).ToList();

            ViewBag.VehicleVariants = variants.Select(v => new SelectListItem
            {
                Value = v.VariantId.ToString(),
                Text = $"{v.VehicleModel?.Manufacturer?.Name} {v.VehicleModel?.Name} {v.Version}"
            }).ToList();
        }

        private async Task LoadTestDriveViewData()
        {
            var dealerId = GetCurrentDealerId();
            var customers = await _customerService.GetCustomersByDealerAsync(dealerId);
            var variants = await _vehicleService.GetAllVehicleVariantsAsync();

            ViewBag.Customers = customers.Select(c => new SelectListItem
            {
                Value = c.CustomerId.ToString(),
                Text = $"{c.FullName} - {c.Phone}"
            }).ToList();

            ViewBag.VehicleVariants = variants.Select(v => new SelectListItem
            {
                Value = v.VariantId.ToString(),
                Text = $"{v.VehicleModel?.Manufacturer?.Name} {v.VehicleModel?.Name} {v.Version}"
            }).ToList();
        }

        #endregion
    }
}