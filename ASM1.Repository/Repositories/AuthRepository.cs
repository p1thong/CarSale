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

        public async Task<bool> Register(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<User> GetUserById(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}
