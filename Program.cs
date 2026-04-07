using Microsoft.EntityFrameworkCore;
using WebsiteBanDoAnVat.Data;
using Microsoft.AspNetCore.Identity;
using WebsiteBanDoAnVat.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Identity 
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

    // THÊM DÒNG NÀY: Cho phép UserName ngắn gọn như "admin"
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSession();

var app = builder.Build();

// 3. Cấu hình Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession(); // Đặt UseSession trước MapRoute

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// --- PHẦN THÊM MỚI: SEED DATA ADMIN (ĐÃ TỐI ƯU ĐỂ ĐĂNG NHẬP BẰNG USERNAME) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // 1. Tạo các Role mặc định
    string[] roleNames = { "Admin", "Customer" };
    foreach (var roleName in roleNames)
    {
        if (!roleManager.RoleExistsAsync(roleName).Result)
        {
            roleManager.CreateAsync(new IdentityRole(roleName)).Wait();
        }
    }

    // 2. Tạo tài khoản Admin
    var adminEmail = "admin@gmail.com";
    var adminID = "admin"; // Tên đăng nhập ngắn gọn

    // Tìm thử theo UserName "admin" xem có chưa
    var adminUser = userManager.FindByNameAsync(adminID).Result;

    if (adminUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = adminID,      // GIỜ ĐÂY USERNAME LÀ "admin"
            Email = adminEmail,
            HoTen = "Quản Trị Viên",
            EmailConfirmed = true,
            IsActive = true
        };

        // Mật khẩu: Admin1@
        var createPowerUser = userManager.CreateAsync(user, "Admin1@").Result;

        if (createPowerUser.Succeeded)
        {
            userManager.AddToRoleAsync(user, "Admin").Wait();
        }
    }
}

app.Run();