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

            // 1. Lấy 4 món bán chạy (Giữ nguyên logic của Bảo)
            viewModel.BestSellers = await _context.MonAns
                .Where(m => m.IsAvailable && m.IsBestSeller)
                .Take(4)
                .ToListAsync();

            // 2. Lấy 3 món mới nhất (Giữ nguyên logic của Bảo)
            viewModel.NewProducts = await _context.MonAns
                .Where(m => m.IsAvailable)
                .OrderByDescending(m => m.Id)
                .Take(3)
                .ToListAsync();

            // 3. THÊM MỚI: Lấy danh mục để hiển thị ở Sidebar bên dưới
            // (Cái này chỉ thêm vào chứ không xóa gì cũ nên không sợ lỗi)
            viewModel.Categories = await _context.Categories.ToListAsync();

            // 4. THÊM MỚI: Lấy danh sách tất cả món ăn (hiện ở phần dưới cùng)
            viewModel.AllProducts = await _context.MonAns
                .Where(m => m.IsAvailable)
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
        // Thêm chữ "async" và đổi kiểu trả về thành Task<IActionResult>
        public async Task<IActionResult> SanPham(string filter)
        {
            var query = _context.MonAns.AsQueryable();

            switch (filter)
            {
                case "banchay":
                    query = query.Where(m => m.IsBestSeller == true);
                    break;
                case "moinhat":
                    query = query.OrderByDescending(m => m.Id);
                    break;
                case "hot":
                    query = query.OrderByDescending(m => m.ViewCount);
                    break;
                case "giamgia":
                    query = query.Where(m => m.Price < 50000);
                    break;
            }

            var dsMonAn = await query.ToListAsync();
            return View(dsMonAn);
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