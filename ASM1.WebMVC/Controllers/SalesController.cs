using ASM1.Repository.Models;
using ASM1.Service.Services.Interfaces;
using ASM1.WebMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace ASM1.WebMVC.Controllers
{
    public class SalesController : Controller
    {
        private readonly ISalesService _salesService;
        private readonly ICustomerRelationshipService _customerService;
        private readonly IVehicleService _vehicleService;

        public SalesController(
            ISalesService salesService,
            ICustomerRelationshipService customerService,
            IVehicleService vehicleService
        )
        {
            _salesService = salesService;
            _customerService = customerService;
            _vehicleService = vehicleService;
        }

        // Customer Management Actions
        public async Task<IActionResult> Customers()
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                var customers = await _salesService.GetCustomersByDealerAsync(dealerId);
                
                // Thêm thông báo nếu không có khách hàng
                if (customers == null || !customers.Any())
                {
                    ViewBag.Message = "Chưa có khách hàng nào. Hãy thêm khách hàng mới!";
                }
                
                return View(customers);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải danh sách khách hàng: {ex.Message}";
                return View(Enumerable.Empty<Customer>());
            }
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
                    var createdCustomer = await _salesService.CreateOrUpdateCustomerAsync(customer);
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
            try
            {
                var customer = await _salesService.GetCustomerAsync(id);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(Customers));
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading customer: {ex.Message}";
                return RedirectToAction(nameof(Customers));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditCustomer(Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _salesService.CreateOrUpdateCustomerAsync(customer);
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

        // Quotation Management Actions
        public async Task<IActionResult> Quotations()
        {
            var dealerId = GetCurrentDealerId();
            var quotations = await _salesService.GetQuotationsByDealerAsync(dealerId);
            
            // Debug log
            Console.WriteLine($"DEBUG: DealerId = {dealerId}, Quotations count = {quotations?.Count() ?? 0}");
            foreach (var q in quotations ?? Enumerable.Empty<Quotation>())
            {
                Console.WriteLine($"  Quotation #{q.QuotationId}: Customer={q.Customer?.FullName}, Status={q.Status}, Price={q.Price}");
            }
            
            return View(quotations);
        }

        [HttpGet]
        public async Task<IActionResult> CreateQuotation(int customerId)
        {
            try
            {
                // Nếu có customerId, lấy thông tin khách hàng
                if (customerId > 0)
                {
                    var customer = await _salesService.GetCustomerAsync(customerId);
                    if (customer == null)
                    {
                        TempData["Error"] = "Không tìm thấy khách hàng.";
                        return RedirectToAction(nameof(Customers));
                    }
                    ViewBag.Customer = customer;
                }
                
                // Lấy danh sách phiên bản xe có sẵn từ DB
                var vehicleVariants = await _vehicleService.GetAvailableVariantsAsync();
                if (vehicleVariants == null || !vehicleVariants.Any())
                {
                    TempData["Warning"] = "Không có phiên bản xe nào có sẵn. Vui lòng thêm xe vào kho trước.";
                }
                ViewBag.VehicleVariants = vehicleVariants;
                
                // Lấy danh sách khách hàng từ DB cho dropdown
                var dealerId = GetCurrentDealerId();
                var customers = await _salesService.GetCustomersByDealerAsync(dealerId);
                if (customers == null || !customers.Any())
                {
                    ViewBag.CustomerMessage = "Chưa có khách hàng nào. Hãy thêm khách hàng trước khi tạo báo giá.";
                }
                ViewBag.Customers = customers;
                
                return View("CreateQuotation");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải form báo giá: {ex.Message}";
                return RedirectToAction(nameof(Customers));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuotation(
            int customerId,
            int variantId,
            decimal price,
            decimal discount = 0
        )
        {
            try
            {
                // Validate input parameters
                if (customerId <= 0)
                {
                    TempData["Error"] = "Please select a valid customer.";
                    return RedirectToAction(nameof(CreateQuotation), new { customerId });
                }

                if (variantId <= 0)
                {
                    TempData["Error"] = "Please select a valid vehicle variant.";
                    return RedirectToAction(nameof(CreateQuotation), new { customerId });
                }

                if (price <= 0)
                {
                    TempData["Error"] = "Price must be greater than 0.";
                    return RedirectToAction(nameof(CreateQuotation), new { customerId });
                }

                if (discount < 0 || discount > 100)
                {
                    TempData["Error"] = "Discount must be between 0% and 100%.";
                    return RedirectToAction(nameof(CreateQuotation), new { customerId });
                }

                var dealerId = GetCurrentDealerId();
                var quotation = await _salesService.CreateQuotationAsync(
                    customerId,
                    variantId,
                    dealerId,
                    price
                );
                TempData["Success"] = "Quotation created successfully!";
                return RedirectToAction(nameof(Quotations));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating quotation: {ex.Message}";
                return RedirectToAction(nameof(CreateQuotation), new { customerId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveQuotation(int quotationId)
        {
            try
            {
                var quotation = await _salesService.ApproveQuotationAsync(quotationId);
                return Json(new { success = true, message = "Quotation approved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Order Management Actions
        public async Task<IActionResult> Orders()
        {
            var dealerId = GetCurrentDealerId();
            var orders = await _salesService.GetOrdersByDealerAsync(dealerId);
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrder(int? customerId, int? variantId)
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                
                // Load customers and vehicles for dropdowns
                var customers = await _salesService.GetCustomersByDealerAsync(dealerId);
                var variants = await _vehicleService.GetAllVehicleVariantsAsync();
                
                ViewBag.Customers = customers;
                ViewBag.VehicleVariants = variants;
                ViewBag.PreselectedCustomerId = customerId;
                ViewBag.PreselectedVariantId = variantId;
                
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading order form: {ex.Message}";
                return RedirectToAction(nameof(Orders));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderModel model)
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                
                var order = new Order
                {
                    DealerId = dealerId,
                    CustomerId = model.CustomerId,
                    VariantId = model.VariantId,
                    Status = "Pending",
                    OrderDate = DateOnly.FromDateTime(DateTime.Now)
                };
                
                await _salesService.CreateOrderAsync(order);
                TempData["Success"] = "Order created successfully!";
                return RedirectToAction(nameof(Orders));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating order: {ex.Message}";
                
                // Reload form data on error
                var dealerId = GetCurrentDealerId();
                var customers = await _salesService.GetCustomersByDealerAsync(dealerId);
                var variants = await _vehicleService.GetAllVehicleVariantsAsync();
                
                ViewBag.Customers = customers;
                ViewBag.VehicleVariants = variants;
                ViewBag.PreselectedCustomerId = model.CustomerId;
                ViewBag.PreselectedVariantId = model.VariantId;
                
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderFromQuotation(int quotationId)
        {
            await _salesService.CreateOrderFromQuotationAsync(quotationId);
            return RedirectToAction(nameof(Orders));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            await _salesService.UpdateOrderStatusAsync(orderId, status);
            return RedirectToAction(nameof(Orders));
        }

        // Payment Management Actions
        public async Task<IActionResult> OrderPayments(int orderId)
        {
            var payments = await _salesService.GetPaymentsByOrderAsync(orderId);
            var remainingBalance = await _salesService.GetRemainingBalanceAsync(orderId);

            ViewBag.OrderId = orderId;
            ViewBag.RemainingBalance = remainingBalance;

            return View(payments);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(
            int orderId,
            decimal amount,
            string paymentMethod
        )
        {
            await _salesService.ProcessPaymentAsync(orderId, amount, paymentMethod);
            return RedirectToAction(nameof(OrderPayments), new { orderId });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var isReady = await _salesService.IsOrderReadyForDeliveryAsync(orderId);
            if (isReady)
            {
                await _salesService.CompleteOrderAsync(orderId);
                TempData["Success"] = "Order completed successfully!";
            }
            else
            {
                TempData["Error"] = "Order is not ready for delivery. Please check payment status.";
            }

            return RedirectToAction(nameof(Orders));
        }

        // Sales Contract Management Actions
        public async Task<IActionResult> SalesContracts()
        {
            var dealerId = GetCurrentDealerId();
            var orders = await _salesService.GetOrdersByDealerAsync(dealerId);
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSalesContract(int orderId)
        {
            try
            {
                var order = await _salesService.GetOrderAsync(orderId);
                if (order == null)
                {
                    TempData["Error"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction(nameof(Orders));
                }

                ViewBag.Order = order;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tạo hợp đồng: {ex.Message}";
                return RedirectToAction(nameof(Orders));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesContract(int orderId, decimal totalAmount, string terms)
        {
            try
            {
                var contract = await _salesService.CreateSalesContractAsync(orderId, totalAmount, terms);
                TempData["Success"] = "Hợp đồng bán xe đã được tạo thành công!";
                return RedirectToAction(nameof(OrderDetail), new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tạo hợp đồng: {ex.Message}";
                return RedirectToAction(nameof(CreateSalesContract), new { orderId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetail(int id)
        {
            try
            {
                var order = await _salesService.GetOrderAsync(id);
                if (order == null)
                {
                    TempData["Error"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction(nameof(Orders));
                }

                var contract = await _salesService.GetSalesContractByOrderAsync(id);
                var payments = await _salesService.GetPaymentsByOrderAsync(id);
                var remainingBalance = await _salesService.GetRemainingBalanceAsync(id);

                ViewBag.SalesContract = contract;
                ViewBag.Payments = payments;
                ViewBag.RemainingBalance = remainingBalance;

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải chi tiết đơn hàng: {ex.Message}";
                return RedirectToAction(nameof(Orders));
            }
        }

        private int GetCurrentDealerId()
        {
            // Implementation to get current dealer ID from session/claims
            // This is a placeholder - implement based on your authentication system
            return HttpContext.Session.GetInt32("DealerId") ?? 1;
        }

        // API endpoints for CreateQuotation form
        [HttpGet]
        public async Task<IActionResult> GetCustomersJson()
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                var customers = await _salesService.GetCustomersByDealerAsync(dealerId);
                return Json(customers.Select(c => new { 
                    value = c.CustomerId, 
                    text = $"{c.FullName} ({c.Email})" 
                }));
            }
            catch
            {
                return Json(new object[0]);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetManufacturersJson()
        {
            try
            {
                var manufacturers = await _vehicleService.GetAllManufacturersAsync();
                return Json(manufacturers.Select(m => new { 
                    value = m.ManufacturerId, 
                    text = m.Name 
                }));
            }
            catch
            {
                return Json(new object[0]);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetModelsByManufacturerJson(int manufacturerId)
        {
            try
            {
                var models = await _vehicleService.GetVehicleModelsByManufacturerAsync(manufacturerId);
                return Json(models.Select(m => new { 
                    value = m.VehicleModelId, 
                    text = m.Name 
                }));
            }
            catch
            {
                return Json(new object[0]);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVariantsByModelJson(int modelId)
        {
            try
            {
                var variants = await _vehicleService.GetVehicleVariantsByModelAsync(modelId);
                return Json(variants.Select(v => new { 
                    value = v.VariantId, 
                    text = $"{v.Version} - {v.Color ?? "N/A"}", 
                    price = v.Price 
                }));
            }
            catch
            {
                return Json(new object[0]);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetQuotationDetailsJson(int quotationId)
        {
            try
            {
                var quotation = await _salesService.GetQuotationAsync(quotationId);
                if (quotation == null)
                {
                    return Json(new { success = false, message = "Quotation not found" });
                }

                return Json(new { 
                    success = true,
                    data = new {
                        quotationId = quotation.QuotationId,
                        status = quotation.Status,
                        price = quotation.Price,
                        createdAt = quotation.CreatedAt?.ToString("dd/MM/yyyy HH:mm"),
                        customer = new {
                            name = quotation.Customer?.FullName ?? "N/A",
                            email = quotation.Customer?.Email ?? "N/A",
                            phone = quotation.Customer?.Phone ?? "N/A"
                        },
                        vehicle = new {
                            model = quotation.Variant?.VehicleModel?.Name ?? "N/A",
                            version = quotation.Variant?.Version ?? "N/A",
                            color = quotation.Variant?.Color ?? "N/A",
                            basePrice = quotation.Variant?.Price ?? 0
                        },
                        dealer = new {
                            name = quotation.Dealer?.FullName ?? "N/A"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
