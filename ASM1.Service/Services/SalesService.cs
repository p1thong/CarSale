using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using ASM1.Service.Services.Interfaces;

namespace ASM1.Service.Services
{
    public class SalesService : ISalesService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IQuotationRepository _quotationRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IVehicleRepository _vehicleRepository;

        public SalesService(
            ICustomerRepository customerRepository,
            IQuotationRepository quotationRepository,
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IVehicleRepository vehicleRepository)
        {
            _customerRepository = customerRepository;
            _quotationRepository = quotationRepository;
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

        // Quotation Management
        public async Task<Quotation> CreateQuotationAsync(Quotation quotation)
        {
            return await _quotationRepository.CreateQuotationAsync(quotation);
        }

        public async Task<Quotation?> GetQuotationByIdAsync(int quotationId)
        {
            return await _quotationRepository.GetQuotationByIdAsync(quotationId);
        }

        public async Task<IEnumerable<Quotation>> GetQuotationsByDealerAsync(int dealerId)
        {
            return await _quotationRepository.GetQuotationsByDealerAsync(dealerId);
        }

        public async Task UpdateQuotationStatusAsync(int quotationId, string status)
        {
            var quotation = await _quotationRepository.GetQuotationByIdAsync(quotationId);
            if (quotation != null)
            {
                quotation.Status = status;
                await _quotationRepository.UpdateQuotationAsync(quotation);
            }
        }

        // Order Management
        public async Task<Order> CreateOrderAsync(Order order)
        {
            return await _orderRepository.CreateOrderAsync(order);
        }

        // Quotation Management
        public async Task<Quotation> CreateQuotationAsync(int customerId, int variantId, int dealerId, decimal price)
        {
            var variant = await _vehicleRepository.GetVehicleVariantByIdAsync(variantId);
            if (variant == null)
                throw new ArgumentException("Vehicle variant not found");

            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found");

            var quotation = new Quotation
            {
                CustomerId = customerId,
                VariantId = variantId,
                DealerId = dealerId,
                Price = price,
                CreatedAt = DateTime.Now,
                Status = "Pending"
            };

            return await _quotationRepository.CreateQuotationAsync(quotation);
        }

        public async Task<Quotation?> GetQuotationAsync(int quotationId)
        {
            return await _quotationRepository.GetQuotationByIdAsync(quotationId);
        }

        public async Task<Quotation> ApproveQuotationAsync(int quotationId)
        {
            var quotation = await _quotationRepository.GetQuotationByIdAsync(quotationId);
            if (quotation == null)
                throw new ArgumentException("Quotation not found");

            quotation.Status = "Approved";
            return await _quotationRepository.UpdateQuotationAsync(quotation);
        }

        public async Task<Quotation> RejectQuotationAsync(int quotationId)
        {
            var quotation = await _quotationRepository.GetQuotationByIdAsync(quotationId);
            if (quotation == null)
                throw new ArgumentException("Quotation not found");

            quotation.Status = "Rejected";
            return await _quotationRepository.UpdateQuotationAsync(quotation);
        }

        public async Task<IEnumerable<Quotation>> GetPendingQuotationsAsync(int dealerId)
        {
            return await _quotationRepository.GetQuotationsByDealerAsync(dealerId);
        }

        // Order Management
        public async Task<Order> CreateOrderFromQuotationAsync(int quotationId)
        {
            var quotation = await _quotationRepository.GetQuotationByIdAsync(quotationId);
            if (quotation == null || quotation.Status != "Approved")
                throw new ArgumentException("Quotation not found or not approved");

            var order = new Order
            {
                DealerId = quotation.DealerId,
                CustomerId = quotation.CustomerId,
                VariantId = quotation.VariantId,
                Status = "Pending",
                OrderDate = DateOnly.FromDateTime(DateTime.Now)
            };

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

            var payment = new Payment
            {
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