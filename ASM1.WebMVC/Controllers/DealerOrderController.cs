using ASM1.Repository.Models;
using ASM1.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASM1.WebMVC.Controllers
{
    public class DealerOrderController : Controller
    {
        private readonly ISalesService _salesService;

        public DealerOrderController(ISalesService salesService)
        {
            _salesService = salesService;
        }

        // Dealer xem danh sách đơn hàng chờ xác nhận
        [HttpGet]
        public async Task<IActionResult> PendingOrders()
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                var pendingOrders = await _salesService.GetPendingOrdersByDealerAsync(dealerId);
                return View(pendingOrders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải danh sách đơn hàng: {ex.Message}";
                return View(Enumerable.Empty<Order>());
            }
        }

        // Dealer xem tất cả đơn hàng
        [HttpGet]
        public async Task<IActionResult> AllOrders()
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                var orders = await _salesService.GetOrdersByDealerAsync(dealerId);
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải danh sách đơn hàng: {ex.Message}";
                return View(Enumerable.Empty<Order>());
            }
        }

        // Dealer xem chi tiết đơn hàng
        [HttpGet]
        public async Task<IActionResult> OrderDetail(int orderId)
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                var order = await _salesService.GetOrderAsync(orderId);
                
                if (order == null || order.DealerId != dealerId)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem.";
                    return RedirectToAction(nameof(AllOrders));
                }

                // Lấy thông tin thanh toán
                var payments = await _salesService.GetPaymentsByOrderAsync(orderId);
                var totalPaid = payments?.Sum(p => p.Amount ?? 0) ?? 0;
                var orderTotal = order.Variant?.Price ?? 0;

                ViewBag.Payments = payments;
                ViewBag.TotalPaid = totalPaid;
                ViewBag.OrderTotal = orderTotal;
                ViewBag.RemainingBalance = orderTotal - totalPaid;

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải chi tiết đơn hàng: {ex.Message}";
                return RedirectToAction(nameof(AllOrders));
            }
        }

        // Dealer xác nhận đơn hàng
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(int orderId, string dealerNotes = "")
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                var order = await _salesService.GetOrderAsync(orderId);
                
                if (order == null || order.DealerId != dealerId)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xử lý.";
                    return RedirectToAction(nameof(PendingOrders));
                }

                if (order.Status != "Pending")
                {
                    TempData["Error"] = "Đơn hàng không ở trạng thái chờ xác nhận.";
                    return RedirectToAction(nameof(OrderDetail), new { orderId });
                }

                await _salesService.ConfirmOrderAsync(orderId, dealerNotes);

                TempData["Success"] = "Đã xác nhận đơn hàng thành công!";
                return RedirectToAction(nameof(OrderDetail), new { orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xác nhận đơn hàng: {ex.Message}";
                return RedirectToAction(nameof(OrderDetail), new { orderId });
            }
        }

        // Dealer từ chối đơn hàng
        [HttpPost]
        public async Task<IActionResult> RejectOrder(int orderId, string rejectionReason)
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                var order = await _salesService.GetOrderAsync(orderId);
                
                if (order == null || order.DealerId != dealerId)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xử lý.";
                    return RedirectToAction(nameof(PendingOrders));
                }

                if (order.Status != "Pending")
                {
                    TempData["Error"] = "Đơn hàng không ở trạng thái chờ xác nhận.";
                    return RedirectToAction(nameof(OrderDetail), new { orderId });
                }

                await _salesService.RejectOrderAsync(orderId, rejectionReason);

                TempData["Success"] = "Đã từ chối đơn hàng.";
                return RedirectToAction(nameof(PendingOrders));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi từ chối đơn hàng: {ex.Message}";
                return RedirectToAction(nameof(OrderDetail), new { orderId });
            }
        }

        // Dealer giao xe cho customer
        [HttpPost]
        public async Task<IActionResult> DeliverOrder(int orderId)
        {
            try
            {
                var dealerId = GetCurrentDealerId();
                var order = await _salesService.GetOrderAsync(orderId);
                
                if (order == null || order.DealerId != dealerId)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xử lý.";
                    return RedirectToAction(nameof(AllOrders));
                }

                // Kiểm tra đã thanh toán đủ chưa
                var payments = await _salesService.GetPaymentsByOrderAsync(orderId);
                var totalPaid = payments?.Sum(p => p.Amount ?? 0) ?? 0;
                var orderTotal = order.Variant?.Price ?? 0;

                if (totalPaid < orderTotal)
                {
                    TempData["Error"] = "Khách hàng chưa thanh toán đủ, không thể giao xe.";
                    return RedirectToAction(nameof(OrderDetail), new { orderId });
                }

                await _salesService.CompleteOrderAsync(orderId);

                TempData["Success"] = "Đã giao xe thành công!";
                return RedirectToAction(nameof(OrderDetail), new { orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi giao xe: {ex.Message}";
                return RedirectToAction(nameof(OrderDetail), new { orderId });
            }
        }

        private int GetCurrentDealerId()
        {
            return HttpContext.Session.GetInt32("DealerId") ?? 1;
        }
    }
}