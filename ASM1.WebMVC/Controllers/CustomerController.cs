using ASM1.Repository.Models;
using ASM1.Service.Services.Interfaces;
using ASM1.Repository.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASM1.WebMVC.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ISalesService _salesService;
        private readonly ICustomerRelationshipService _customerService;
        private readonly IQuotationRepository _quotationRepository;
        private readonly IOrderRepository _orderRepository;

        public CustomerController(
            ISalesService salesService,
            ICustomerRelationshipService customerService,
            IQuotationRepository quotationRepository,
            IOrderRepository orderRepository)
        {
            _salesService = salesService;
            _customerService = customerService;
            _quotationRepository = quotationRepository;
            _orderRepository = orderRepository;
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
                var quotations = await _quotationRepository.GetQuotationsByCustomerAsync(customerId.Value);
                var orders = await _orderRepository.GetOrdersByCustomerAsync(customerId.Value);

                ViewBag.Customer = customer;
                ViewBag.QuotationsCount = quotations.Count();
                ViewBag.OrdersCount = orders.Count();
                ViewBag.PendingQuotationsCount = quotations.Count(q => q.Status == "Pending");
                ViewBag.ApprovedQuotationsCount = quotations.Count(q => q.Status == "Approved");

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading customer portal: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // Customer Quotations
        public async Task<IActionResult> MyQuotations()
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                TempData["Error"] = "Please login to view your quotations.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var quotations = await _quotationRepository.GetQuotationsByCustomerAsync(customerId.Value);
                return View(quotations);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading quotations: {ex.Message}";
                return View(Enumerable.Empty<Quotation>());
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
                var orders = await _orderRepository.GetOrdersByCustomerAsync(customerId.Value);
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading orders: {ex.Message}";
                return View(Enumerable.Empty<Order>());
            }
        }

        // View Quotation Details
        public async Task<IActionResult> QuotationDetails(int id)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                TempData["Error"] = "Please login to view quotation details.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var quotation = await _salesService.GetQuotationAsync(id);
                if (quotation == null || quotation.CustomerId != customerId)
                {
                    TempData["Error"] = "Quotation not found or access denied.";
                    return RedirectToAction(nameof(MyQuotations));
                }

                return View(quotation);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading quotation details: {ex.Message}";
                return RedirectToAction(nameof(MyQuotations));
            }
        }

        // Accept Quotation (Customer accepts the quotation price)
        [HttpPost]
        public async Task<IActionResult> AcceptQuotation(int quotationId)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                return Json(new { success = false, message = "Please login to accept quotation." });
            }

            try
            {
                var quotation = await _salesService.GetQuotationAsync(quotationId);
                if (quotation == null || quotation.CustomerId != customerId)
                {
                    return Json(new { success = false, message = "Quotation not found or access denied." });
                }

                if (quotation.Status != "Approved")
                {
                    return Json(new { success = false, message = "Only approved quotations can be accepted." });
                }

                // Create order from quotation
                var order = new Order
                {
                    CustomerId = customerId.Value,
                    VariantId = quotation.VariantId,
                    DealerId = quotation.DealerId,
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
                    Status = "Pending"
                };

                await _salesService.CreateOrderAsync(order);

                // Update quotation status
                await _salesService.UpdateQuotationStatusAsync(quotationId, "Accepted");

                return Json(new { success = true, message = "Quotation accepted and order created successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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