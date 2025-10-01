using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASM1.WebMVC.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Kiểm tra cookie authentication trước
            if (User.Identity?.IsAuthenticated == true)
            {
                // Lấy thông tin từ claims
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                // Đồng bộ với session nếu session rỗng
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")) && !string.IsNullOrEmpty(userId))
                {
                    HttpContext.Session.SetString("UserId", userId);
                    HttpContext.Session.SetString("UserRole", userRole ?? "Guest");
                    HttpContext.Session.SetString("UserName", userName ?? "Unknown");
                }

                // Set ViewBag
                ViewBag.IsLoggedIn = true;
                ViewBag.UserId = userId;
                ViewBag.UserRole = userRole;
                ViewBag.UserName = userName;
            }
            else
            {
                // Kiểm tra session (fallback)
                var userId = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");
                var userName = HttpContext.Session.GetString("UserName");

                if (!string.IsNullOrEmpty(userId))
                {
                    ViewBag.IsLoggedIn = true;
                    ViewBag.UserId = userId;
                    ViewBag.UserRole = userRole;
                    ViewBag.UserName = userName;
                }
                else
                {
                    ViewBag.IsLoggedIn = false;
                    
                    // Redirect to login if trying to access protected pages
                    var controller = context.RouteData.Values["controller"]?.ToString();
                    var action = context.RouteData.Values["action"]?.ToString();
                    
                    // Only allow access to Auth controller (Login/Register)
                    if (controller != "Auth")
                    {
                        context.Result = RedirectToAction("Login", "Auth");
                        return;
                    }
                }
            }

            base.OnActionExecuting(context);
        }
    }
}