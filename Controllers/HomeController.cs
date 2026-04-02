using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm thư viện này để dùng ToListAsync()
using WebsiteBanDoAnVat.Models;
using WebsiteBanDoAnVat.Data;

namespace WebsiteBanDoAnVat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context; // Bổ sung AppDbContext để gọi Database

        // Inject cả Logger và DbContext vào Constructor
        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // 1. Trang Chủ (Đã cập nhật để lấy dữ liệu thật từ SQL)
        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            // Lấy 4 món bán chạy (IsAvailable = true và IsBestSeller = true)
            viewModel.BestSellers = await _context.MonAns
                .Where(m => m.IsAvailable && m.IsBestSeller)
                .Take(4)
                .ToListAsync();

            // Lấy 3 món mới nhất (IsAvailable = true, sắp xếp Id giảm dần)
            viewModel.NewProducts = await _context.MonAns
                .Where(m => m.IsAvailable)
                .OrderByDescending(m => m.Id)
                .Take(3)
                .ToListAsync();

            // Truyền dữ liệu sang View
            return View(viewModel);
        }

        // 2. Trang Giới Thiệu
        public IActionResult GioiThieu()
        {
            return View();
        }

        // 3. Trang Sản Phẩm
        public IActionResult SanPham()
        {
            return View();
        }

        // 4. Trang Tin Tức
        public IActionResult TinTuc()
        {
            return View();
        }

        // 5. Trang Liên Hệ
        public IActionResult LienHe()
        {
            return View();
        }
        public IActionResult Cart()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}