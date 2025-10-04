using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using ASM1.Service.Services.Interfaces;

namespace ASM1.Service.Services
{
    public class SalesService : ISalesService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IVehicleRepository _vehicleRepository;

        public SalesService(
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IVehicleRepository vehicleRepository)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _vehicleRepository = vehicleRepository;
        }

        // Customer Management
        public async Task<Customer> CreateOrUpdateCustomerAsync(Customer customer)
        {
            if (customer.CustomerId == 0)
            {
                return await _customerRepository.CreateCustomerAsync(customer);
            }
            else
            {
                return await _customerRepository.UpdateCustomerAsync(customer);
            }
        }

        public async Task<Customer?> GetCustomerAsync(int customerId)
        {
            return await _customerRepository.GetCustomerByIdAsync(customerId);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByDealerAsync(int dealerId)
        {
            return await _customerRepository.GetCustomersByDealerAsync(dealerId);
        }

        // Order Management
        public async Task<Order> CreateOrderAsync(Order order)
        {
            return await _orderRepository.CreateOrderAsync(order);
        }

        public async Task<Order?> GetOrderAsync(int orderId)
        {
            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

        public async Task<Order> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found");

            order.Status = status;
            return await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task<IEnumerable<Order>> GetOrdersByDealerAsync(int dealerId)
        {
            return await _orderRepository.GetOrdersByDealerAsync(dealerId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            return await _orderRepository.GetOrdersByCustomerAsync(customerId);
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersByDealerAsync(int dealerId)
        {
            var allOrders = await _orderRepository.GetOrdersByDealerAsync(dealerId);
            return allOrders.Where(o => o.Status == "Pending");
        }

        public async Task<Order> ConfirmOrderAsync(int orderId, string dealerNotes = "")
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found");

            if (order.Status != "Pending")
                throw new InvalidOperationException("Order is not in pending status");

            order.Status = "Confirmed";
            // Note: You might want to add a DealerNotes field to Order model
            return await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task<Order> RejectOrderAsync(int orderId, string rejectionReason)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found");

            if (order.Status != "Pending")
                throw new InvalidOperationException("Order is not in pending status");

            order.Status = "Rejected";
            // Note: You might want to add a RejectionReason field to Order model
            return await _orderRepository.UpdateOrderAsync(order);
        }

        // Sales Contract Management
        public async Task<SalesContract> CreateSalesContractAsync(int orderId, decimal totalAmount, string terms)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found");

            var contract = new SalesContract
            {
                OrderId = orderId,
                ContractDate = DateOnly.FromDateTime(DateTime.Now),
                TotalAmount = totalAmount,
                Terms = terms,
                Status = "Active"
            };

            // Note: You might need to create SalesContractRepository if not exists
            // For now, we'll assume it's handled through Order navigation property
            order.Status = "Contract Signed";
            await _orderRepository.UpdateOrderAsync(order);

            return contract;
        }

        public async Task<SalesContract?> GetSalesContractByOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            return order?.SalesContracts.FirstOrDefault();
        }

        // Payment Management
        public async Task<Payment> ProcessPaymentAsync(int orderId, decimal amount, string paymentMethod)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found");

            // Generate unique PaymentId
            var existingPayments = await _paymentRepository.GetAllPaymentsAsync();
            var maxPaymentId = existingPayments.Any() ? existingPayments.Max(p => p.PaymentId) : 0;

            var payment = new Payment
            {
                PaymentId = maxPaymentId + 1,
                OrderId = orderId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                PaymentDate = DateTime.Now,
                Status = "Completed"
            };

            return await _paymentRepository.CreatePaymentAsync(payment);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByOrderAsync(int orderId)
        {
            return await _paymentRepository.GetPaymentsByOrderAsync(orderId);
        }

        public async Task<decimal> GetRemainingBalanceAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null) return 0;

            var totalPaid = await _paymentRepository.GetTotalPaidAmountByOrderAsync(orderId);
            var totalAmount = order.SalesContracts.FirstOrDefault()?.TotalAmount ?? 0;

            return totalAmount - totalPaid;
        }

        // Vehicle Delivery
        public async Task<Order> CompleteOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found");

            var remainingBalance = await GetRemainingBalanceAsync(orderId);
            if (remainingBalance > 0)
                throw new InvalidOperationException("Order has outstanding balance");

            order.Status = "Completed";
            return await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task<bool> IsOrderReadyForDeliveryAsync(int orderId)
        {
            var remainingBalance = await GetRemainingBalanceAsync(orderId);
            return remainingBalance <= 0;
        }
    }
}