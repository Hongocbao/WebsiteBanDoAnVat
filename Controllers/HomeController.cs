using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebsiteBanDoAnVat.Models;

namespace WebsiteBanDoAnVat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // 1. Trang Chủ
        public IActionResult Index()
        {
            return View();
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