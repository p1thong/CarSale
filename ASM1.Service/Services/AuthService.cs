using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using ASM1.Service.Services.Interfaces;

namespace ASM1.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;

        public AuthService(IAuthRepository repository)
        {
            _repository = repository;
        }

        public bool CheckRole(User user, string requiredRole)
        {
            return _repository.CheckRole(user, requiredRole);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _repository.GetUserByEmail(email);
        }

        public async Task<User> Login(string email, string password)
        {
            return await _repository.Login(email, password);
        }

        public async Task<bool> Register(User user, int? dealerId = null)
        {
            return await _repository.Register(user, dealerId);
        }

        public async Task<User> GetUserById(int userId)
        {
            return await _repository.GetUserById(userId);
        }
    }
}
