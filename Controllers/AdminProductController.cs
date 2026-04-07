using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteBanDoAnVat.Data;
using WebsiteBanDoAnVat.Models;

namespace WebsiteBanDoAnVat.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        // BỔ SUNG: Khai báo UserManager để quản lý khách hàng
        private readonly UserManager<ApplicationUser> _userManager;

        // Cập nhật Constructor để nhận UserManager
        public AdminProductController(
            AppDbContext context,
            IWebHostEnvironment hostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
        }

        // ==========================================
        // 1. DANH SÁCH & THÊM MỚI
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var products = await _context.MonAns.Include(m => m.Category).ToListAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonAn monAn, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    monAn.ImageUrl = await SaveImage(imageFile);
                }
                _context.Add(monAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", monAn.CategoryId);
            return View(monAn);
        }

        // ==========================================
        // 2. CẬP NHẬT (EDIT)
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", monAn.CategoryId);
            return View(monAn);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MonAn monAn, IFormFile? imageFile)
        {
            if (id != monAn.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        if (!string.IsNullOrEmpty(monAn.ImageUrl))
                        {
                            DeleteOldImage(monAn.ImageUrl);
                        }
                        monAn.ImageUrl = await SaveImage(imageFile);
                    }
                    _context.Update(monAn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonAnExists(monAn.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", monAn.CategoryId);
            return View(monAn);
        }

        // ==========================================
        // 3. XÓA (DELETE MÓN ĂN)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn != null)
            {
                if (!string.IsNullOrEmpty(monAn.ImageUrl))
                {
                    DeleteOldImage(monAn.ImageUrl);
                }
                _context.MonAns.Remove(monAn);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // QUẢN LÝ ĐƠN HÀNG
        // ==========================================
        public async Task<IActionResult> OrderManagement()
        {
            var orders = await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        public IActionResult DoanhThu()
        {
            decimal tongDoanhThu = _context.Orders.Sum(o => o.TotalAmount);
            int tongDonHang = _context.Orders.Count();

            ViewBag.TotalRevenue = tongDoanhThu;
            ViewBag.TotalOrders = tongDonHang;

            var recentOrders = _context.Orders.OrderByDescending(o => o.OrderDate).Take(10).ToList();
            return View(recentOrders);
        }

        // ==========================================
        // TRANG KHÁCH HÀNG (Sửa lỗi _userManager)
        // ==========================================
        public async Task<IActionResult> KhachHang(string searchTerm)
        {
            var query = _userManager.Users.Where(u => u.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => (u.HoTen != null && u.HoTen.Contains(searchTerm))
                                      || (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm))
                                      || (u.MaSinhVien != null && u.MaSinhVien.Contains(searchTerm)));
                ViewBag.SearchTerm = searchTerm;
            }

            var list = await query.OrderByDescending(u => u.NgayDangKy).ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteKhachHang(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(KhachHang));
        }

        public async Task<IActionResult> ChiTietHoaDon(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(d => d.MonAn)
                .FirstOrDefaultAsync(m => m.Id == id);
            return order == null ? NotFound() : View(order);
        }

        // Helper Methods
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
            string filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create)) { await imageFile.CopyToAsync(stream); }
            return "/images/" + fileName;
        }

        private void DeleteOldImage(string imageUrl)
        {
            string oldPath = Path.Combine(_hostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        }

        private bool MonAnExists(int id) => _context.MonAns.Any(e => e.Id == id);
    }
}