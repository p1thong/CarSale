namespace ASM1.WebMVC.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();
            var userId = context.Session.GetString("UserId");

            // Danh sách các trang không cần đăng nhập
            var allowedPaths = new[]
            {
                "/",
                "/home",
                "/home/index",
                "/home/privacy",
                "/auth/login",
                "/auth/register",
                "/css/",
                "/js/",
                "/lib/",
                "/images/",
                "/favicon.ico"
            };

            // Kiểm tra nếu là static files hoặc allowed paths
            bool isAllowed = allowedPaths.Any(p => path.StartsWith(p)) || 
                           path.Contains(".css") || 
                           path.Contains(".js") || 
                           path.Contains(".png") || 
                           path.Contains(".jpg") || 
                           path.Contains(".ico");

            // Nếu chưa đăng nhập và truy cập trang cần bảo vệ
            if (string.IsNullOrEmpty(userId) && !isAllowed)
            {
                context.Response.Redirect("/Auth/Login");
                return;
            }

            await _next(context);
        }
    }
}