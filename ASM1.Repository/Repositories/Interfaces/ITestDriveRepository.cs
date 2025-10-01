using ASM1.Repository.Models;

namespace ASM1.Repository.Repositories.Interfaces
{
    public interface ITestDriveRepository
    {
        Task<IEnumerable<TestDrive>> GetAllTestDrivesAsync();
        Task<TestDrive?> GetTestDriveByIdAsync(int testDriveId);
        Task<IEnumerable<TestDrive>> GetTestDrivesByCustomerAsync(int customerId);
        Task<IEnumerable<TestDrive>> GetTestDrivesByDealerAsync(int dealerId);
        Task<IEnumerable<TestDrive>> GetTestDrivesByStatusAsync(string status);
        Task<TestDrive> CreateTestDriveAsync(TestDrive testDrive);
        Task<TestDrive> UpdateTestDriveAsync(TestDrive testDrive);
        Task<bool> DeleteTestDriveAsync(int testDriveId);
        Task<IEnumerable<TestDrive>> GetScheduledTestDrivesAsync(DateOnly date);
    }
}