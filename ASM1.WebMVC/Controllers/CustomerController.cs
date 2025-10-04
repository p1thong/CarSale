using ASM1.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASM1.WebMVC.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ISalesService _salesService;
        private readonly ICustomerRelationshipService _customerService;

        public CustomerController(
            ISalesService salesService,
            ICustomerRelationshipService customerService)
        {
            _salesService = salesService;
            _customerService = customerService;
        }

        // Customer Portal Home
        public async Task<IActionResult> Portal()
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                TempData["Error"] = "Please login to access customer portal.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var customer = await _salesService.GetCustomerAsync(customerId.Value);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction("Login", "Auth");
                }

                // Get customer stats
                var orders = await _salesService.GetOrdersByCustomerAsync(customerId.Value);

                ViewBag.Customer = customer;
                ViewBag.OrdersCount = orders.Count();
                ViewBag.PendingOrdersCount = orders.Count(o => o.Status == "Pending");
                ViewBag.ConfirmedOrdersCount = orders.Count(o => o.Status == "Confirmed");
                ViewBag.CompletedOrdersCount = orders.Count(o => o.Status == "Completed");

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading customer portal: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // Customer Orders
        public async Task<IActionResult> MyOrders()
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                TempData["Error"] = "Please login to view your orders.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var orders = await _salesService.GetOrdersByCustomerAsync(customerId.Value);
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading orders: {ex.Message}";
                return View(Enumerable.Empty<Order>());
            }
        }

        // Payment for Order
        public async Task<IActionResult> PaymentOptions(int orderId)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                TempData["Error"] = "Please login to make payment.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var order = await _salesService.GetOrderAsync(orderId);
                if (order == null || order.CustomerId != customerId)
                {
                    TempData["Error"] = "Order not found or access denied.";
                    return RedirectToAction(nameof(MyOrders));
                }

                // Get existing payments for this order
                var payments = await _salesService.GetPaymentsByOrderAsync(orderId);
                var remainingBalance = await _salesService.GetRemainingBalanceAsync(orderId);

                ViewBag.Order = order;
                ViewBag.Payments = payments;
                ViewBag.RemainingBalance = remainingBalance;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading payment options: {ex.Message}";
                return RedirectToAction(nameof(MyOrders));
            }
        }

        // Process Payment
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int orderId, decimal amount, string paymentMethod)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                return Json(new { success = false, message = "Please login to make payment." });
            }

            try
            {
                var order = await _salesService.GetOrderAsync(orderId);
                if (order == null || order.CustomerId != customerId)
                {
                    return Json(new { success = false, message = "Order not found or access denied." });
                }

                if (amount <= 0)
                {
                    return Json(new { success = false, message = "Payment amount must be greater than 0." });
                }

                // Get remaining balance before payment
                var remainingBalanceBefore = await _salesService.GetRemainingBalanceAsync(orderId);

                if (amount > remainingBalanceBefore)
                {
                    return Json(new { success = false, message = "Payment amount cannot exceed remaining balance." });
                }

                // Process payment using the service method
                await _salesService.ProcessPaymentAsync(orderId, amount, paymentMethod);

                // Get remaining balance after payment
                var remainingBalanceAfter = await _salesService.GetRemainingBalanceAsync(orderId);

                // Check if order is fully paid
                if (remainingBalanceAfter <= 0)
                {
                    await _salesService.UpdateOrderStatusAsync(orderId, "Paid");
                }

                return Json(new { 
                    success = true, 
                    message = "Payment processed successfully!",
                    remainingBalance = remainingBalanceAfter
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int? GetCurrentCustomerId()
        {
            // Implementation to get current customer ID from session/claims
            // This should be implemented based on your authentication system
            return HttpContext.Session.GetInt32("CustomerId");
        }
    }
}