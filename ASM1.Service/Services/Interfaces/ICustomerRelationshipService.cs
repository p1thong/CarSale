using ASM1.Repository.Models;

namespace ASM1.Service.Services.Interfaces
{
    public interface ICustomerRelationshipService
    {
        // Customer Management
        Task<IEnumerable<Customer>> GetCustomersByDealerAsync(int dealerId);
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);

        // Test Drive Management
        Task<TestDrive> ScheduleTestDriveAsync(
            int customerId,
            int variantId,
            DateOnly scheduledDate,
            TimeOnly? scheduledTime = null
        );
        Task<TestDrive> ConfirmTestDriveAsync(int testDriveId);
        Task<TestDrive> CompleteTestDriveAsync(int testDriveId);
        Task<TestDrive?> GetTestDriveByIdAsync(int testDriveId);
        Task<IEnumerable<TestDrive>> GetTestDriveScheduleAsync(DateOnly date);
        Task<IEnumerable<TestDrive>> GetAllTestDrivesAsync();
        Task<IEnumerable<TestDrive>> GetCustomerTestDrivesAsync(int customerId);
        Task<IEnumerable<TestDrive>> GetTestDrivesByDealerAsync(int dealerId);
        Task<TestDrive> CreateTestDriveAsync(TestDrive testDrive);
        Task UpdateTestDriveStatusAsync(int testDriveId, string status);

        // Feedback Management
        Task<Feedback> CreateFeedbackAsync(int customerId, string content, int rating);
        Task<IEnumerable<Feedback>> GetCustomerFeedbacksAsync(int customerId);
        Task<IEnumerable<Feedback>> GetAllFeedbacksAsync();
        Task<IEnumerable<Feedback>> GetFeedbacksByDealerAsync(int dealerId);

        // Customer Profile & History
        Task<CustomerProfileDto> GetCustomerProfileAsync(int customerId);
        Task<IEnumerable<Order>> GetCustomerOrderHistoryAsync(int customerId);
        Task<IEnumerable<Payment>> GetCustomerPaymentHistoryAsync(int customerId);
        Task<decimal> GetCustomerOutstandingBalanceAsync(int customerId);

        // Customer Care & Promotions
        Task<IEnumerable<Promotion>> GetCustomerEligiblePromotionsAsync(int customerId);
        Task<CustomerCareReportDto> GenerateCustomerCareReportAsync(
            int dealerId,
            DateTime fromDate,
            DateTime toDate
        );
    }

    public class CustomerProfileDto
    {
        public required Customer Customer { get; set; }
        public required IEnumerable<Order> Orders { get; set; }
        public required IEnumerable<TestDrive> TestDrives { get; set; }
        public required IEnumerable<Feedback> Feedbacks { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
    }

    public class CustomerCareReportDto
    {
        public int TotalCustomers { get; set; }
        public int TotalTestDrives { get; set; }
        public int TotalFeedbacks { get; set; }
        public double AverageRating { get; set; }
        public decimal TotalSales { get; set; }
        public required IEnumerable<Customer> NewCustomers { get; set; }
        public required IEnumerable<Feedback> RecentFeedbacks { get; set; }
    }
}
