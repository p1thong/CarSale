using ASM1.Repository.Data;
using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASM1.Repository.Repositories
{
    public class TestDriveRepository : ITestDriveRepository
    {
        private readonly CarSalesDbContext _context;

        public TestDriveRepository(CarSalesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestDrive>> GetAllTestDrivesAsync()
        {
            return await _context
                .TestDrives.Include(t => t.Customer)
                .Include(t => t.Variant)
                .ThenInclude(v => v.VehicleModel)
                .OrderByDescending(t => t.TestDriveId)
                .ToListAsync();
        }

        public async Task<TestDrive?> GetTestDriveByIdAsync(int testDriveId)
        {
            return await _context
                .TestDrives.Include(t => t.Customer)
                .Include(t => t.Variant)
                .ThenInclude(v => v.VehicleModel)
                .FirstOrDefaultAsync(t => t.TestDriveId == testDriveId);
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByCustomerAsync(int customerId)
        {
            return await _context
                .TestDrives.Where(t => t.CustomerId == customerId)
                .Include(t => t.Variant)
                .ThenInclude(v => v.VehicleModel)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByDealerAsync(int dealerId)
        {
            return await _context
                .TestDrives.Where(t => t.Customer.DealerId == dealerId)
                .Include(t => t.Customer)
                .Include(t => t.Variant)
                .ThenInclude(v => v.VehicleModel)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByStatusAsync(string status)
        {
            return await _context
                .TestDrives.Where(t => t.Status == status)
                .Include(t => t.Customer)
                .Include(t => t.Variant)
                .ThenInclude(v => v.VehicleModel)
                .ToListAsync();
        }

        public async Task<TestDrive> CreateTestDriveAsync(TestDrive testDrive)
        {
            // Get the next available TestDriveId
            var maxTestDriveId = await _context.TestDrives.MaxAsync(t => (int?)t.TestDriveId) ?? 0;
            testDrive.TestDriveId = maxTestDriveId + 1;
            
            testDrive.Status = "Scheduled";
            _context.TestDrives.Add(testDrive);
            await _context.SaveChangesAsync();
            return testDrive;
        }

        public async Task<TestDrive> UpdateTestDriveAsync(TestDrive testDrive)
        {
            _context.Entry(testDrive).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return testDrive;
        }

        public async Task<bool> DeleteTestDriveAsync(int testDriveId)
        {
            var testDrive = await _context.TestDrives.FindAsync(testDriveId);
            if (testDrive == null)
                return false;

            _context.TestDrives.Remove(testDrive);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TestDrive>> GetScheduledTestDrivesAsync(DateOnly date)
        {
            return await _context
                .TestDrives.Where(t => t.ScheduledDate == date)
                .Include(t => t.Customer)
                .Include(t => t.Variant)
                .ThenInclude(v => v.VehicleModel)
                .OrderByDescending(t => t.TestDriveId)
                .ToListAsync();
        }
    }
}
