using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASM1.WebMVC.Middleware
{
    public class RoleAuthorizationAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizationAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userRole = session.GetString("UserRole");
            var userId = session.GetString("UserId");

            // Check if user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Check if user has required role
            if (!_allowedRoles.Contains(userRole))
            {
                context.Result = new ForbidResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    // Specific role attributes for easier use
    public class AdminOnlyAttribute : RoleAuthorizationAttribute
    {
        public AdminOnlyAttribute() : base("Admin") { }
    }

    public class EVMOnlyAttribute : RoleAuthorizationAttribute
    {
        public EVMOnlyAttribute() : base("Admin", "EVM") { }
    }

    public class DealerOnlyAttribute : RoleAuthorizationAttribute
    {
        public DealerOnlyAttribute() : base("DealerManager", "DealerStaff") { }
    }

    public class DealerManagerOnlyAttribute : RoleAuthorizationAttribute
    {
        public DealerManagerOnlyAttribute() : base("DealerManager") { }
    }

    public class DealerStaffOnlyAttribute : RoleAuthorizationAttribute
    {
        public DealerStaffOnlyAttribute() : base("DealerManager", "DealerStaff") { }
    }

    public class CustomerOnlyAttribute : RoleAuthorizationAttribute
    {
        public CustomerOnlyAttribute() : base("Customer") { }
    }
}