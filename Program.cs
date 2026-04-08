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
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession(); // Đặt UseSession trước MapRoute

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // 1. Tạo Role Admin
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // 2. Tạo tài khoản Admin
    var adminID = "admin"; // Tên đăng nhập ngắn gọn
    var adminEmail = "admin@gmail.com";

    var adminUser = await userManager.FindByNameAsync(adminID);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminID,      // Đăng nhập bằng chữ "admin"
            Email = adminEmail,
            HoTen = "Quản Trị Viên",
            IsAdmin = true,          // Quyền Quản trị (màu vàng)
            IsSuperAdmin = true,     // QUYỀN TỐI CAO (màu đỏ)
            EmailConfirmed = true,
            NgayDangKy = DateTime.Now
        };

        var result = await userManager.CreateAsync(adminUser, "Admin1@");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.Run();
