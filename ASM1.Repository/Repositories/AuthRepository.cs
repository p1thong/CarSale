using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASM1.Repository.Data;
using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASM1.Repository.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly CarSalesDbContext _context;

        public AuthRepository(CarSalesDbContext context)
        {
            _context = context;
        }

        public bool CheckRole(User user, string requiredRole)
        {
            return user.Role.Equals(requiredRole, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == email && u.Password == password
            );

            return user;
        }

        public async Task<bool> Register(User user, int? selectedDealerId = null)
        {
            try
            {
                // Nếu không có dealerId được cung cấp, chọn dealer đầu tiên
                if (!selectedDealerId.HasValue)
                {
                    var firstDealer = await _context.Dealers.FirstOrDefaultAsync();
                    if (firstDealer == null)
                    {
                        return false; // Không có dealer nào trong hệ thống
                    }
                    selectedDealerId = firstDealer.DealerId;
                }

                // Validate dealer exists
                var dealerExists = await _context.Dealers.AnyAsync(d => d.DealerId == selectedDealerId.Value);
                if (!dealerExists)
                {
                    return false;
                }

                // Set DealerId for user
                user.DealerId = selectedDealerId.Value;

                // Thêm User vào database
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // Nếu role là customer, tự động tạo bản ghi Customer
                if (user.Role.Equals("customer", StringComparison.OrdinalIgnoreCase))
                {
                    var customer = new Customer
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Phone = user.Phone,
                        DealerId = selectedDealerId.Value,
                        Birthday = null // Can be updated later by the customer
                    };

                    await _context.Customers.AddAsync(customer);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception)
            {
                // Log the exception if needed
                return false;
            }
        }

        public async Task<User> GetUserById(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}
