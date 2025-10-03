using ASM1.Repository.Models;
using ASM1.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASM1.WebMVC.Controllers
{
    public class CustomerOrderController : Controller
    {
        private readonly ISalesService _salesService;
        private readonly IVehicleService _vehicleService;
        private readonly ICustomerRelationshipService _customerService;

        public CustomerOrderController(
            ISalesService salesService,
            IVehicleService vehicleService,
            ICustomerRelationshipService customerService)
        {
            _salesService = salesService;
            _vehicleService = vehicleService;
            _customerService = customerService;
        }

        // Customer chọn xe và nhập thông tin đặt hàng
        [HttpGet]
        public async Task<IActionResult> CreateOrder(int variantId)
        {
            try
            {
                // Lấy thông tin xe
                var variant = await _vehicleService.GetVehicleVariantByIdAsync(variantId);
                if (variant == null)
                {
                    TempData["Error"] = "Không tìm thấy xe được chọn.";
                    return RedirectToAction("Index", "Home");
                }

                // Lấy thông tin customer từ session
                var customerId = GetCurrentCustomerId();
                if (customerId == 0)
                {
                    TempData["Error"] = "Vui lòng đăng nhập để đặt hàng.";
                    return RedirectToAction("Login", "Auth");
                }

                var customer = await _salesService.GetCustomerAsync(customerId);
                
                ViewBag.Variant = variant;
                ViewBag.Customer = customer;
                ViewBag.MaxQuantity = variant.Quantity;
                
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải form đặt hàng: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // Customer submit đơn hàng
        [HttpPost]
        public async Task<IActionResult> CreateOrder(int variantId, int quantity, string notes = "")
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                if (customerId == 0)
                {
                    TempData["Error"] = "Vui lòng đăng nhập để đặt hàng.";
                    return RedirectToAction("Login", "Auth");
                }

                // Validate quantity
                var variant = await _vehicleService.GetVehicleVariantByIdAsync(variantId);
                if (variant == null)
                {
                    TempData["Error"] = "Không tìm thấy xe được chọn.";
                    return RedirectToAction(nameof(CreateOrder), new { variantId });
                }

                if (quantity <= 0 || quantity > variant.Quantity)
                {
                    TempData["Error"] = $"Số lượng không hợp lệ. Chỉ có {variant.Quantity} xe có sẵn.";
                    return RedirectToAction(nameof(CreateOrder), new { variantId });
                }

                // Lấy dealer của customer
                var customer = await _salesService.GetCustomerAsync(customerId);
                if (customer == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction("Login", "Auth");
                }

                // Tạo đơn hàng với status "Pending" - chờ dealer xác nhận
                var order = new Order
                {
                    CustomerId = customerId,
                    DealerId = customer.DealerId,
                    VariantId = variantId,
                    Status = "Pending", // Chờ dealer xác nhận
                    OrderDate = DateOnly.FromDateTime(DateTime.Now)
                    // Notes = notes - nếu có field này trong model
                };

                var createdOrder = await _salesService.CreateOrderAsync(order);

                TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng: #{createdOrder.OrderId}. Đơn hàng đang chờ dealer xác nhận.";
                return RedirectToAction(nameof(MyOrders));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi đặt hàng: {ex.Message}";
                return RedirectToAction(nameof(CreateOrder), new { variantId });
            }
        }

        // Customer xem trạng thái đơn hàng
        [HttpGet]
        public async Task<IActionResult> OrderStatus(int orderId)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var order = await _salesService.GetOrderAsync(orderId);
                
                if (order == null || order.CustomerId != customerId)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem.";
                    return RedirectToAction("Index", "Home");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải thông tin đơn hàng: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // Customer xem danh sách đơn hàng của mình
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                
                // Debug logging
                ViewBag.SessionCustomerId = HttpContext.Session.GetInt32("CustomerId");
                ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
                ViewBag.ComputedCustomerId = customerId;
                
                if (customerId == 0)
                {
                    TempData["Error"] = "Vui lòng đăng nhập để xem đơn hàng.";
                    // Debug: Thay vì redirect, hiển thị view với thông tin debug
                    ViewBag.Error = "Customer ID = 0 (not logged in)";
                    return View(Enumerable.Empty<Order>());
                }

                var orders = await _salesService.GetOrdersByCustomerAsync(customerId);
                ViewBag.OrdersCount = orders.Count();
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải danh sách đơn hàng: {ex.Message}";
                ViewBag.Error = ex.Message;
                return View(Enumerable.Empty<Order>());
            }
        }

        // Customer thanh toán đơn hàng đã được xác nhận
        [HttpGet]
        public async Task<IActionResult> Payment(int orderId)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var order = await _salesService.GetOrderAsync(orderId);
                
                if (order == null || order.CustomerId != customerId)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem.";
                    return RedirectToAction(nameof(MyOrders));
                }

                if (order.Status != "Confirmed")
                {
                    TempData["Error"] = "Đơn hàng chưa được xác nhận, không thể thanh toán.";
                    return RedirectToAction(nameof(OrderStatus), new { orderId });
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
                return RedirectToAction(nameof(MyOrders));
            }
        }

        // Customer thực hiện thanh toán
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int orderId, decimal amount, string paymentMethod = "Cash")
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var order = await _salesService.GetOrderAsync(orderId);
                
                if (order == null || order.CustomerId != customerId)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền thanh toán.";
                    return RedirectToAction(nameof(MyOrders));
                }

                if (amount <= 0)
                {
                    TempData["Error"] = "Số tiền thanh toán phải lớn hơn 0.";
                    return RedirectToAction(nameof(Payment), new { orderId });
                }

                await _salesService.ProcessPaymentAsync(orderId, amount, paymentMethod);

                TempData["Success"] = $"Thanh toán {amount:C0} thành công!";
                
                // Kiểm tra xem đã thanh toán đủ chưa
                var payments = await _salesService.GetPaymentsByOrderAsync(orderId);
                var totalPaid = payments?.Sum(p => p.Amount ?? 0) ?? 0;
                var orderTotal = order.Variant?.Price ?? 0;
                
                if (totalPaid >= orderTotal)
                {
                    TempData["Success"] = "Thanh toán hoàn tất! Đơn hàng của bạn đã được thanh toán đầy đủ.";
                    return RedirectToAction(nameof(MyOrders));
                }
                
                return RedirectToAction(nameof(Payment), new { orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi thanh toán: {ex.Message}";
                return RedirectToAction(nameof(Payment), new { orderId });
            }
        }

        // Debug action để test MyOrders
        [HttpGet]
        public async Task<IActionResult> TestMyOrders()
        {
            try
            {
                // Debug info
                var sessionCustomerId = HttpContext.Session.GetInt32("CustomerId");
                var userRole = HttpContext.Session.GetString("UserRole");
                
                ViewBag.SessionCustomerId = sessionCustomerId;
                ViewBag.UserRole = userRole;
                
                // Nếu không có customer ID trong session, dùng customer ID = 1 để test
                var customerId = sessionCustomerId ?? 1;
                
                var orders = await _salesService.GetOrdersByCustomerAsync(customerId);
                ViewBag.OrdersCount = orders.Count();
                
                return View("MyOrders", orders);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("MyOrders", Enumerable.Empty<Order>());
            }
        }

        private int GetCurrentCustomerId()
        {
            // Lấy customer ID từ session hoặc tìm theo email
            var sessionCustomerId = HttpContext.Session.GetInt32("CustomerId");
            if (sessionCustomerId.HasValue && sessionCustomerId.Value > 0)
            {
                return sessionCustomerId.Value;
            }

            // Nếu không có CustomerId trong session, tìm theo email
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                try
                {
                    var customer = _customerService.GetCustomerByEmailAsync(userEmail).Result;
                    if (customer != null)
                    {
                        // Lưu lại CustomerId vào session cho lần sau
                        HttpContext.Session.SetInt32("CustomerId", customer.CustomerId);
                        return customer.CustomerId;
                    }
                }
                catch
                {
                    // Ignore error và return 0
                }
            }

            return 0;
        }
    }
}