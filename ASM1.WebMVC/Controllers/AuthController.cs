using ASM1.Repository.Models;
using ASM1.Service.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASM1.WebMVC.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _service;
        private readonly IDealerService _dealerService;

        public AuthController(ILogger<AuthController> logger, IAuthService service, IDealerService dealerService)
        {
            _logger = logger;
            _service = service;
            _dealerService = dealerService;
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            // Kiểm tra nếu đã đăng nhập thì redirect về trang chủ
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _service.Login(email, password);

            if (user == null)
            {
                ViewBag.Error = "Sai email hoặc mật khẩu";
                return View();
            }

            // Tạo claims cho cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CarSalesCookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Đăng nhập với cookie authentication
            await HttpContext.SignInAsync("CarSalesCookies", claimsPrincipal);

            // Lưu thông tin user vào session (backup)
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserRole", user.Role.ToString());
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);

            // Thêm thông báo thành công
            TempData["SuccessMessage"] = $"Đăng nhập thành công! Chào mừng {user.FullName} ({user.Role})";

            // Redirect về trang chủ
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(
            string fullName,
            string email,
            string phone,
            string password,
            string confirmPassword
        )
        {
            // Kiểm tra mật khẩu xác nhận
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp";
                return View();
            }

            // Kiểm tra email đã tồn tại
            var existingUser = await _service.GetUserByEmail(email);
            if (existingUser != null)
            {
                ViewBag.Error = "Email đã được sử dụng";
                return View();
            }

            // Tự động chọn dealer đầu tiên
            var dealers = await _dealerService.GetAllDealersAsync();
            var firstDealer = dealers.FirstOrDefault();
            if (firstDealer == null)
            {
                ViewBag.Error = "Hệ thống chưa có đại lý nào. Vui lòng liên hệ admin.";
                return View();
            }

            // Tạo user mới với vai trò mặc định là customer
            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                Password = password,
                Role = "customer",
            };

            var result = await _service.Register(newUser, firstDealer.DealerId);
            if (result)
            {
                ViewBag.Success = "Đăng ký thành công! Vui lòng đăng nhập.";
                return View();
            }
            else
            {
                ViewBag.Error = "Đăng ký thất bại. Vui lòng thử lại sau.";
                return View();
            }
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            var userName = HttpContext.Session.GetString("UserName") ?? User.Identity?.Name;
            
            // Đăng xuất cookie authentication
            await HttpContext.SignOutAsync("CarSalesCookies");
            
            // Xóa session
            HttpContext.Session.Clear();
            
            TempData["InfoMessage"] = $"Đã đăng xuất thành công! Hẹn gặp lại {userName}.";
            return RedirectToAction("Login", "Auth");
        }
    }
}
