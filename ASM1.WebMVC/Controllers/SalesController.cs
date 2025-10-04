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

                // Debug information
                ViewBag.DealerId = dealerId;
                ViewBag.CustomerCount = customers?.Count() ?? 0;

                // Thêm thông báo nếu không có khách hàng
                if (customers == null || !customers.Any())
                {
                    ViewBag.Message =
                        $"Chưa có khách hàng nào cho dealer ID {dealerId}. Hãy thêm khách hàng mới!";
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

        [HttpPost]
        public async Task<IActionResult> CreateSampleCustomers()
        {
            try
            {
                var dealerId = GetCurrentDealerId();

                // Create sample customers
                var sampleCustomers = new List<Customer>
                {
                    new Customer
                    {
                        DealerId = dealerId,
                        FullName = "Nguyễn Văn A",
                        Email = "nguyenvana@gmail.com",
                        Phone = "0123456789",
                        Birthday = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                    },
                    new Customer
                    {
                        DealerId = dealerId,
                        FullName = "Trần Thị B",
                        Email = "tranthib@gmail.com",
                        Phone = "0987654321",
                        Birthday = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
                    },
                    new Customer
                    {
                        DealerId = dealerId,
                        FullName = "Lê Văn C",
                        Email = "levanc@gmail.com",
                        Phone = "0369852147",
                        Birthday = DateOnly.FromDateTime(DateTime.Now.AddYears(-35)),
                    },
                };

                foreach (var customer in sampleCustomers)
                {
                    await _salesService.CreateOrUpdateCustomerAsync(customer);
                }

                TempData["Success"] =
                    $"Đã tạo {sampleCustomers.Count} khách hàng mẫu cho dealer ID {dealerId}!";
                return RedirectToAction(nameof(Customers));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tạo khách hàng mẫu: {ex.Message}";
                return RedirectToAction(nameof(Customers));
            }
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

        // ========== ORDER & PAYMENT FLOW ==========
        
        [HttpGet]
        public async Task<IActionResult> CreateOrderDirect(int? customerId = null)
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                
                // Load data for dropdowns
                var customers = await _salesService.GetCustomersByDealerAsync(dealerId);
                var variants = await _vehicleService.GetAvailableVariantsAsync();
                
                ViewBag.Customers = customers;
                ViewBag.VehicleVariants = variants;
                ViewBag.SelectedCustomerId = customerId;
                ViewBag.DealerId = dealerId;
                
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải form tạo hóa đơn: {ex.Message}";
                return RedirectToAction(nameof(Customers));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderDirect(int customerId, int variantId, int quantity = 1)
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                
                // Validate
                if (customerId <= 0 || variantId <= 0)
                {
                    TempData["Error"] = "Vui lòng chọn khách hàng và xe.";
                    return RedirectToAction(nameof(CreateOrderDirect), new { customerId });
                }
                
                // Check variant availability
                var variant = await _vehicleService.GetVehicleVariantByIdAsync(variantId);
                if (variant == null)
                {
                    TempData["Error"] = "Không tìm thấy xe được chọn.";
                    return RedirectToAction(nameof(CreateOrderDirect), new { customerId });
                }
                
                if (variant.Quantity < quantity)
                {
                    TempData["Error"] = $"Số lượng yêu cầu ({quantity}) vượt quá tồn kho ({variant.Quantity}).";
                    return RedirectToAction(nameof(CreateOrderDirect), new { customerId });
                }
                
                // Create order
                var order = new Order
                {
                    DealerId = dealerId,
                    CustomerId = customerId,
                    VariantId = variantId,
                    Status = "Pending",
                    OrderDate = DateOnly.FromDateTime(DateTime.Now)
                };
                
                var createdOrder = await _salesService.CreateOrderAsync(order);
                
                TempData["Success"] = $"Tạo hóa đơn #{createdOrder.OrderId} thành công!";
                return RedirectToAction(nameof(OrderPayment), new { orderId = createdOrder.OrderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tạo hóa đơn: {ex.Message}";
                return RedirectToAction(nameof(CreateOrderDirect), new { customerId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderPayment(int orderId)
        {
            try
            {
                var order = await _salesService.GetOrderAsync(orderId);
                if (order == null)
                {
                    TempData["Error"] = "Không tìm thấy hóa đơn.";
                    return RedirectToAction(nameof(Orders));
                }
                
                var payments = await _salesService.GetPaymentsByOrderAsync(orderId);
                var totalPaid = payments?.Sum(p => p.Amount ?? 0) ?? 0;
                var orderTotal = order.Variant?.Price ?? 0;
                var remainingBalance = orderTotal - totalPaid;
                
                ViewBag.Order = order;
                ViewBag.Payments = payments;
                ViewBag.TotalPaid = totalPaid;
                ViewBag.OrderTotal = orderTotal;
                ViewBag.RemainingBalance = remainingBalance;
                ViewBag.IsFullyPaid = remainingBalance <= 0;
                
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải trang thanh toán: {ex.Message}";
                return RedirectToAction(nameof(Orders));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPaymentDirect(int orderId, decimal amount, string paymentMethod = "Cash")
        {
            try
            {
                if (amount <= 0)
                {
                    TempData["Error"] = "Số tiền thanh toán phải lớn hơn 0.";
                    return RedirectToAction(nameof(OrderPayment), new { orderId });
                }
                
                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = amount,
                    PaymentMethod = paymentMethod,
                    PaymentDate = DateTime.Now,
                    Status = "Completed"
                };
                
                await _salesService.ProcessPaymentAsync(orderId, amount, paymentMethod);
                
                TempData["Success"] = $"Thanh toán {amount:C0} thành công!";
                return RedirectToAction(nameof(OrderPayment), new { orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi thanh toán: {ex.Message}";
                return RedirectToAction(nameof(OrderPayment), new { orderId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrderDirect(int orderId)
        {
            try
            {
                var order = await _salesService.GetOrderAsync(orderId);
                var payments = await _salesService.GetPaymentsByOrderAsync(orderId);
                var totalPaid = payments?.Sum(p => p.Amount ?? 0) ?? 0;
                var orderTotal = order?.Variant?.Price ?? 0;
                
                if (totalPaid < orderTotal)
                {
                    TempData["Error"] = $"Chưa thanh toán đủ. Còn thiếu: {(orderTotal - totalPaid):C0}";
                    return RedirectToAction(nameof(OrderPayment), new { orderId });
                }
                
                await _salesService.UpdateOrderStatusAsync(orderId, "Completed");
                
                TempData["Success"] = "Hoàn thành hóa đơn thành công!";
                return RedirectToAction(nameof(Orders));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi hoàn thành hóa đơn: {ex.Message}";
                return RedirectToAction(nameof(OrderPayment), new { orderId });
            }
        }

        // Order Management Actions
        public async Task<IActionResult> Orders()
        {
            var dealerId = GetCurrentDealerId();
            var orders = await _salesService.GetOrdersByDealerAsync(dealerId);
            
            // Debug info
            ViewBag.CurrentDealerId = dealerId;
            ViewBag.OrderCount = orders?.Count() ?? 0;
            
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
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
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
        public async Task<IActionResult> CreateSalesContract(
            int orderId,
            decimal totalAmount,
            string terms
        )
        {
            try
            {
                var contract = await _salesService.CreateSalesContractAsync(
                    orderId,
                    totalAmount,
                    terms
                );
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
    }
}
