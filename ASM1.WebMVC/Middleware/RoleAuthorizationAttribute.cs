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

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"[DEBUG] UserRole: {userRole}, UserId: {userId}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Required roles: {string.Join(", ", _allowedRoles)}");

            // Check if user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] User not logged in - redirecting to login");
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Check if user has required role (case-insensitive comparison)
            if (!_allowedRoles.Any(role => string.Equals(role, userRole, StringComparison.OrdinalIgnoreCase)))
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Access denied - user role '{userRole}' not in allowed roles");
                context.Result = new ForbidResult();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Access granted for role '{userRole}'");
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
        public DealerOnlyAttribute() : base("Dealer", "DealerManager", "DealerStaff") { }
    }

    public class DealerManagerOnlyAttribute : RoleAuthorizationAttribute
    {
        public DealerManagerOnlyAttribute() : base("Dealer", "DealerManager") { }
    }

    public class DealerStaffOnlyAttribute : RoleAuthorizationAttribute
    {
        public DealerStaffOnlyAttribute() : base("Dealer", "DealerManager", "DealerStaff") { }
    }

    public class CustomerOnlyAttribute : RoleAuthorizationAttribute
    {
        public CustomerOnlyAttribute() : base("Customer" , "customer") { }
    }

    public class CustomerAndDealerAttribute : RoleAuthorizationAttribute
    {
        public CustomerAndDealerAttribute() : base("Customer", "Dealer", "DealerManager", "DealerStaff") { }
    }
}