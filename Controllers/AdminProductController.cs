using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteBanDoAnVat.Data;
using WebsiteBanDoAnVat.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebsiteBanDoAnVat.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly AppDbContext _context;

        public AdminProductController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _context.MonAns.Include(m => m.Category).ToListAsync();
            return View(products);
        }

        // 2. QUẢN LÝ ĐƠN HÀNG (MỚI THÊM)
        public async Task<IActionResult> OrderManagement()
        {
            // Lấy danh sách đơn hàng khớp với Model Order của Bảo (CustomerName, PhoneNumber...)
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // 3. CHI TIẾT HÓA ĐƠN (MỚI THÊM)
        public async Task<IActionResult> ChiTietHoaDon(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(d => d.MonAn)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // Các hàm CRUD Sản phẩm (Create, Edit, Delete...) giữ nguyên như code cũ của bạn
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
                if (imageFile != null) monAn.ImageUrl = await SaveImage(imageFile);
                _context.Add(monAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(monAn);
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create)) { await imageFile.CopyToAsync(stream); }
            return "/images/" + fileName;
        }

        // (Thêm các hàm Edit, Delete của bạn vào đây...)
    }
}