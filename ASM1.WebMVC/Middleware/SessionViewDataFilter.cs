using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASM1.WebMVC.Middleware
{
    public class SessionViewDataFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                var httpContext = context.HttpContext;
                var userId = httpContext.Session.GetString("UserId");
                var userRole = httpContext.Session.GetString("UserRole");
                var userName = httpContext.Session.GetString("UserName");

                if (!string.IsNullOrEmpty(userId))
                {
                    controller.ViewBag.IsLoggedIn = true;
                    controller.ViewBag.UserId = userId;
                    controller.ViewBag.UserRole = userRole;
                    controller.ViewBag.UserName = userName;
                }
                else
                {
                    controller.ViewBag.IsLoggedIn = false;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Nothing to do after action execution
        }
    }
}