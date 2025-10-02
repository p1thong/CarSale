using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASM1.Repository.Models;

namespace ASM1.Repository.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User> Login(string email, string password);
        bool CheckRole(User user, string requiredRole);
        Task<User> GetUserByEmail(string email);
        Task<bool> Register(User user, int? dealerId = null);
        Task<User> GetUserById(int userId);
    }
}
