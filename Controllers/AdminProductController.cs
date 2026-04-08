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
        private readonly UserManager<ApplicationUser> _userManager;

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
        // 1. QUẢN LÝ SẢN PHẨM (MÓN ĂN)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var products = await _context.MonAns.Include(m => m.Category).AsNoTracking().ToListAsync();
            return View("Index", products);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonAn monAn, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null) monAn.ImageUrl = await SaveImage(imageFile, "products");
                _context.Add(monAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", monAn.CategoryId);
            return View("Create", monAn);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn == null) return NotFound();
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", monAn.CategoryId);
            return View("Edit", monAn);
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
                        if (!string.IsNullOrEmpty(monAn.ImageUrl)) DeleteOldImage(monAn.ImageUrl);
                        monAn.ImageUrl = await SaveImage(imageFile, "products");
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
            return View("Edit", monAn);
        }

        // --- HÀM XÓA SẢN PHẨM (BỔ SUNG) ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var monAn = await _context.MonAns.Include(m => m.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (monAn == null) return NotFound();
            return View(monAn);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn != null)
            {
                if (!string.IsNullOrEmpty(monAn.ImageUrl)) DeleteOldImage(monAn.ImageUrl);
                _context.MonAns.Remove(monAn);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 2. QUẢN LÝ ĐƠN HÀNG & DOANH THU
        // ==========================================
        public async Task<IActionResult> OrderManagement()
        {
            var orders = await _context.Orders.OrderByDescending(o => o.OrderDate).AsNoTracking().ToListAsync();
            return View("~/Views/Home/OrderManagement.cshtml", orders);
        }

        public async Task<IActionResult> OrderDetail(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(d => d.MonAn)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();
            return View("OrderDetail", order);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(OrderDetail), new { id = id });
        }

        public async Task<IActionResult> DoanhThu(DateTime? tuNgay, DateTime? denNgay, string loaiBaoCao)
        {
            // 1. Lấy truy vấn cơ bản (Chỉ tính những đơn đã "Hoàn thành" thì doanh thu mới chuẩn)
            var query = _context.Orders.Where(o => o.Status == "Hoàn thành").AsQueryable();

            // 2. Lọc theo thời gian nếu Admin chọn trên giao diện
            if (tuNgay.HasValue)
            {
                query = query.Where(o => o.OrderDate >= tuNgay.Value);
            }
            if (denNgay.HasValue)
            {
                // Thêm 1 ngày để bao gồm cả dữ liệu của ngày kết thúc
                var endDate = denNgay.Value.AddDays(1);
                query = query.Where(o => o.OrderDate < endDate);
            }
            if (loaiBaoCao == "thang")
            {
                // Chúng ta lấy dữ liệu thô về trước (ToList), sau đó mới định dạng chuỗi ở trên RAM
                var rawStats = await query
                    .GroupBy(o => new { o.OrderDate.Month, o.OrderDate.Year })
                    .Select(g => new {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        DoanhThu = g.Sum(o => o.TotalAmount),
                        SoDon = g.Count()
                    })
                    .ToListAsync();

                // Bây giờ mới biến thành dạng chuỗi "Tháng/Năm" để hiển thị
                var statsByMonth = rawStats.Select(s => new {
                    Thang = $"{s.Month}/{s.Year}",
                    DoanhThu = s.DoanhThu,
                    SoDon = s.SoDon
                }).ToList();

                ViewBag.StatsByMonth = statsByMonth;
            }

            // 3. Lấy danh sách đã lọc
            var orders = await query.OrderByDescending(o => o.OrderDate).AsNoTracking().ToListAsync();

            // 4. Tính toán các con số tổng quát
            decimal totalRevenue = orders.Sum(o => o.TotalAmount);
            int totalOrders = orders.Count;

            // Giả sử lợi nhuận = 30% doanh thu (Ông có thể sửa số 0.3 này tùy ý)
            decimal estimatedProfit = totalRevenue * 0.3m;

            // 5. Gửi dữ liệu qua ViewBag để hiển thị lên mấy cái Box màu sắc
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.EstimatedProfit = estimatedProfit;

            // Gửi thêm ngày đã chọn để hiển thị lại trên ô Input sau khi load trang
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");

            // 6. Trả về View cùng với danh sách 10 giao dịch gần nhất
            var recentOrders = orders.Take(10).ToList();
            return View("DoanhThu", recentOrders);
        }

        // ==========================================
        // 3. KHÁCH HÀNG & PHÂN QUYỀN
        // ==========================================
        public async Task<IActionResult> KhachHang(string searchTerm)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => (u.HoTen != null && u.HoTen.Contains(searchTerm))
                                      || (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm))
                                      || (u.Email != null && u.Email.Contains(searchTerm)));
            }

            var list = await query.OrderByDescending(u => u.NgayDangKy).ToListAsync();
            return View("KhachHang", list);
        }

        public async Task<IActionResult> UserDetail(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // --- HÀM ĐỔI QUYỀN (FIX LỖI 404 CHANGERÔLE) ---
        [HttpPost]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !currentUser.IsSuperAdmin)
            {
                TempData["Error"] = "Chỉ SuperAdmin mới có quyền này!";
                return RedirectToAction(nameof(KhachHang));
            }

            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null) return NotFound();

            if (targetUser.IsAdmin)
            {
                targetUser.IsAdmin = false;
                await _userManager.RemoveFromRoleAsync(targetUser, "Admin");
                await _userManager.AddToRoleAsync(targetUser, "Customer");
            }
            else
            {
                targetUser.IsAdmin = true;
                await _userManager.RemoveFromRoleAsync(targetUser, "Customer");
                await _userManager.AddToRoleAsync(targetUser, "Admin");
            }

            await _userManager.UpdateAsync(targetUser);
            return RedirectToAction(nameof(KhachHang));
        }

        // ==========================================
        // HELPER METHODS (XỬ LÝ ẢNH)
        // ==========================================
        private async Task<string> SaveImage(IFormFile imageFile, string subFolder)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images", subFolder);

            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            string path = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/{subFolder}/{fileName}";
        }

        private void DeleteOldImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl.Contains("default-avatar")) return;
            string fullPath = Path.Combine(_hostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
        }

        private bool MonAnExists(int id) => _context.MonAns.Any(e => e.Id == id);
    }
}