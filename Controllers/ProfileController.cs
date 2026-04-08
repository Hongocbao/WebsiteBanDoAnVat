using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebsiteBanDoAnVat.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Thêm để dùng ToListAsync
using WebsiteBanDoAnVat.Data; // Thêm để dùng AppDbContext

namespace WebsiteBanDoAnVat.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context; // Thêm context để lấy đơn hàng

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment, AppDbContext context)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _context = context; // Khởi tạo context
        }

        // Trang hiển thị hồ sơ (Sửa lỗi 404 khi vào /Profile)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string hoTen, string phoneNumber, string diaChi, IFormFile avatarFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.HoTen = hoTen;
            user.PhoneNumber = phoneNumber;
            user.DiaChi = diaChi;

            if (avatarFile != null && avatarFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");

                // Tự động tạo thư mục nếu chưa có
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(avatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream);
                }

                user.AvatarUrl = "/images/avatars/" + uniqueFileName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction("Index");
            }

            return View("Index", user);
        }

        // ==========================================================
        // NEW: TRANG THEO DÕI ĐƠN HÀNG (DỰA TRÊN PHONE NUMBER)
        // ==========================================================
        public async Task<IActionResult> TrackingOrder()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            // Lấy danh sách đơn hàng dựa trên số điện thoại của người dùng đang đăng nhập
            var orders = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(d => d.MonAn)
                .Where(o => o.PhoneNumber == user.PhoneNumber) // Khớp theo số điện thoại trong hồ sơ
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();

            return View(orders);
        }
    }
}