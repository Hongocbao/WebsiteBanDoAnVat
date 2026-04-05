using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanDoAnVat.Models;
using WebsiteBanDoAnVat.Data;

namespace WebsiteBanDoAnVat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // 1. Trang Chủ
        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();
            viewModel.BestSellers = await _context.MonAns.Where(m => m.IsAvailable && m.IsBestSeller).Take(4).ToListAsync();
            viewModel.NewProducts = await _context.MonAns.Where(m => m.IsAvailable).OrderByDescending(m => m.Id).Take(3).ToListAsync();
            viewModel.Categories = await _context.Categories.ToListAsync();
            viewModel.AllProducts = await _context.MonAns.Where(m => m.IsAvailable).ToListAsync();
            return View(viewModel);
        }

        // 2. Trang Chi Tiết Món Ăn
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var monAn = await _context.MonAns
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (monAn == null) return NotFound();

            return View(monAn);
        }

        // 3. Trang Sản Phẩm
        public async Task<IActionResult> SanPham(string filter)
        {
            var query = _context.MonAns.AsQueryable();
            switch (filter)
            {
                case "banchay": query = query.Where(m => m.IsBestSeller); break;
                case "moinhat": query = query.OrderByDescending(m => m.Id); break;
                case "giamgia": query = query.Where(m => m.Price < 50000); break;
            }
            return View(await query.ToListAsync());
        }

        // 4. Trang Giỏ Hàng (Giải quyết lỗi 404)
        public IActionResult Cart()
        {
            return View();
        }

        // Các trang phụ khác
        public IActionResult GioiThieu() => View();
        public IActionResult TinTuc() => View();
        public IActionResult LienHe() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}