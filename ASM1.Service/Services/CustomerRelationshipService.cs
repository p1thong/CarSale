using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using ASM1.Service.Services.Interfaces;

namespace ASM1.Service.Services
{
    public class CustomerRelationshipService : ICustomerRelationshipService
    {
        private readonly ITestDriveRepository _testDriveRepository;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;

        public CustomerRelationshipService(
            ITestDriveRepository testDriveRepository,
            IFeedbackRepository feedbackRepository,
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository)
        {
            _testDriveRepository = testDriveRepository;
            _feedbackRepository = feedbackRepository;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
        }

        // Customer Management
        public async Task<IEnumerable<Customer>> GetCustomersByDealerAsync(int dealerId)
        {
            return await _customerRepository.GetCustomersByDealerAsync(dealerId);
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            return await _customerRepository.GetCustomerByIdAsync(customerId);
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _customerRepository.GetCustomerByEmailAsync(email);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            return await _customerRepository.CreateCustomerAsync(customer);
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            return await _customerRepository.UpdateCustomerAsync(customer);
        }

        // Test Drive Management
        public async Task<TestDrive> ScheduleTestDriveAsync(int customerId, int variantId, DateOnly scheduledDate, TimeOnly? scheduledTime = null)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found");

            var testDrive = new TestDrive
            {
                CustomerId = customerId,
                VariantId = variantId,
                ScheduledDate = scheduledDate,
                ScheduledTime = scheduledTime,
                Status = "Scheduled",
            };

            return await _testDriveRepository.CreateTestDriveAsync(testDrive);
        }

        public async Task<TestDrive> CreateTestDriveAsync(TestDrive testDrive)
        {
            return await _testDriveRepository.CreateTestDriveAsync(testDrive);
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByDealerAsync(int dealerId)
        {
            return await _testDriveRepository.GetTestDrivesByDealerAsync(dealerId);
        }

        public async Task UpdateTestDriveStatusAsync(int testDriveId, string status)
        {
            var testDrive = await _testDriveRepository.GetTestDriveByIdAsync(testDriveId);
            if (testDrive != null)
            {
                testDrive.Status = status;
                await _testDriveRepository.UpdateTestDriveAsync(testDrive);
            }
        }

        public async Task<TestDrive> ConfirmTestDriveAsync(int testDriveId)
        {
            var testDrive = await _testDriveRepository.GetTestDriveByIdAsync(testDriveId);
            if (testDrive == null)
                throw new ArgumentException("Test drive not found");

            testDrive.Status = "Confirmed";
            return await _testDriveRepository.UpdateTestDriveAsync(testDrive);
        }

        public async Task<TestDrive> CompleteTestDriveAsync(int testDriveId)
        {
            var testDrive = await _testDriveRepository.GetTestDriveByIdAsync(testDriveId);
            if (testDrive == null)
                throw new ArgumentException("Test drive not found");

            testDrive.Status = "Completed";
            return await _testDriveRepository.UpdateTestDriveAsync(testDrive);
        }

        public async Task<IEnumerable<TestDrive>> GetTestDriveScheduleAsync(DateOnly date)
        {
            return await _testDriveRepository.GetScheduledTestDrivesAsync(date);
        }

        public async Task<IEnumerable<TestDrive>> GetAllTestDrivesAsync()
        {
            return await _testDriveRepository.GetAllTestDrivesAsync();
        }

        public async Task<TestDrive?> GetTestDriveByIdAsync(int testDriveId)
        {
            return await _testDriveRepository.GetTestDriveByIdAsync(testDriveId);
        }

        public async Task<IEnumerable<TestDrive>> GetCustomerTestDrivesAsync(int customerId)
        {
            return await _testDriveRepository.GetTestDrivesByCustomerAsync(customerId);
        }

        // Feedback Management
        public async Task<Feedback> CreateFeedbackAsync(int customerId, string content, int rating)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found");

            var feedback = new Feedback
            {
                CustomerId = customerId,
                Content = content,
                Rating = rating,
                FeedbackDate = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            return await _feedbackRepository.CreateFeedbackAsync(feedback);
        }

        public async Task<IEnumerable<Feedback>> GetCustomerFeedbacksAsync(int customerId)
        {
            return await _feedbackRepository.GetFeedbacksByCustomerAsync(customerId);
        }

        public async Task<IEnumerable<Feedback>> GetAllFeedbacksAsync()
        {
            return await _feedbackRepository.GetAllFeedbacksAsync();
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByDealerAsync(int dealerId)
        {
            return await _feedbackRepository.GetFeedbacksByDealerAsync(dealerId);
        }

        // Customer Profile & History
        public async Task<CustomerProfileDto> GetCustomerProfileAsync(int customerId)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found");

            var orders = await _orderRepository.GetOrdersByCustomerAsync(customerId);
            var testDrives = await _testDriveRepository.GetTestDrivesByCustomerAsync(customerId);
            var feedbacks = await _feedbackRepository.GetFeedbacksByCustomerAsync(customerId);

            var totalPurchaseAmount = orders.Where(o => o.Status == "Completed")
                .Sum(o => o.SalesContracts.Sum(sc => sc.TotalAmount ?? 0));

            var outstandingBalance = 0m;
            foreach (var order in orders.Where(o => o.Status != "Completed"))
            {
                var totalPaid = await _paymentRepository.GetTotalPaidAmountByOrderAsync(order.OrderId);
                var orderTotal = order.SalesContracts.Sum(sc => sc.TotalAmount ?? 0);
                outstandingBalance += orderTotal - totalPaid;
            }

            return new CustomerProfileDto
            {
                Customer = customer,
                Orders = orders,
                TestDrives = testDrives,
                Feedbacks = feedbacks,
                TotalPurchaseAmount = totalPurchaseAmount,
                OutstandingBalance = outstandingBalance
            };
        }

        public async Task<IEnumerable<Order>> GetCustomerOrderHistoryAsync(int customerId)
        {
            return await _orderRepository.GetOrdersByCustomerAsync(customerId);
        }

        public async Task<IEnumerable<Payment>> GetCustomerPaymentHistoryAsync(int customerId)
        {
            var orders = await _orderRepository.GetOrdersByCustomerAsync(customerId);
            var payments = new List<Payment>();

            foreach (var order in orders)
            {
                var orderPayments = await _paymentRepository.GetPaymentsByOrderAsync(order.OrderId);
                payments.AddRange(orderPayments);
            }

            return payments.OrderByDescending(p => p.PaymentDate);
        }

        public async Task<decimal> GetCustomerOutstandingBalanceAsync(int customerId)
        {
            var profile = await GetCustomerProfileAsync(customerId);
            return profile.OutstandingBalance;
        }

        // Customer Care & Promotions
        public Task<IEnumerable<Promotion>> GetCustomerEligiblePromotionsAsync(int customerId)
        {
            // Since we don't have IPromotionRepository yet, query from context directly
            // This is a temporary implementation - should be moved to repository layer
            try
            {
                // For now, return all active promotions
                // In reality, this would filter based on customer eligibility
                return Task.FromResult<IEnumerable<Promotion>>(new List<Promotion>()); // TODO: Implement proper promotion repository
            }
            catch
            {
                return Task.FromResult<IEnumerable<Promotion>>(new List<Promotion>());
            }
        }

        public async Task<CustomerCareReportDto> GenerateCustomerCareReportAsync(int dealerId, DateTime fromDate, DateTime toDate)
        {
            var customers = await _customerRepository.GetCustomersByDealerAsync(dealerId);
            var newCustomers = customers.Where(c => c.Orders.Any(o => o.OrderDate >= DateOnly.FromDateTime(fromDate) && o.OrderDate <= DateOnly.FromDateTime(toDate)));

            var allTestDrives = new List<TestDrive>();
            var allFeedbacks = new List<Feedback>();
            var totalSales = 0m;

            foreach (var customer in customers)
            {
                var customerTestDrives = await _testDriveRepository.GetTestDrivesByCustomerAsync(customer.CustomerId);
                var customerFeedbacks = await _feedbackRepository.GetFeedbacksByCustomerAsync(customer.CustomerId);
                var customerOrders = await _orderRepository.GetOrdersByCustomerAsync(customer.CustomerId);

                allTestDrives.AddRange(customerTestDrives.Where(td => td.ScheduledDate >= DateOnly.FromDateTime(fromDate) && td.ScheduledDate <= DateOnly.FromDateTime(toDate)));
                allFeedbacks.AddRange(customerFeedbacks);

                totalSales += customerOrders.Where(o => o.Status == "Completed" && o.OrderDate >= DateOnly.FromDateTime(fromDate) && o.OrderDate <= DateOnly.FromDateTime(toDate))
                    .Sum(o => o.SalesContracts.Sum(sc => sc.TotalAmount ?? 0));
            }

            var averageRating = allFeedbacks.Any() ? allFeedbacks.Average(f => f.Rating ?? 0) : 0;

            return new CustomerCareReportDto
            {
                TotalCustomers = customers.Count(),
                TotalTestDrives = allTestDrives.Count(),
                TotalFeedbacks = allFeedbacks.Count(),
                AverageRating = averageRating,
                TotalSales = totalSales,
                NewCustomers = newCustomers,
                RecentFeedbacks = allFeedbacks.OrderByDescending(f => f.FeedbackDate).Take(10)
            };
        }
    }
}