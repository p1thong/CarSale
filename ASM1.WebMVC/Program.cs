using ASM1.Repository.Data;
using ASM1.Repository.Repositories;
using ASM1.Repository.Repositories.Interfaces;
using ASM1.Service.Services;
using ASM1.Service.Services.Interfaces;
using ASM1.WebMVC.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Add global filter to handle session data for all controllers
    options.Filters.Add<SessionViewDataFilter>();
});

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Cookie Authentication
builder
    .Services.AddAuthentication("CarSalesCookies")
    .AddCookie(
        "CarSalesCookies",
        options =>
        {
            options.LoginPath = "/Auth/Login";
            options.LogoutPath = "/Auth/Logout";
            options.ExpireTimeSpan = TimeSpan.FromDays(7); // Cookie sống 7 ngày
            options.SlidingExpiration = true; // Gia hạn cookie khi user hoạt động
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        }
    );

// Add HttpContextAccessor for session access
builder.Services.AddHttpContextAccessor();

//add connection String
builder.Services.AddDbContext<CarSalesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

//add repositories
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IDealerRepository, DealerRepository>();
builder.Services.AddScoped<ITestDriveRepository, TestDriveRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

//add services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<ICustomerRelationshipService, CustomerRelationshipService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IDealerService, DealerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
