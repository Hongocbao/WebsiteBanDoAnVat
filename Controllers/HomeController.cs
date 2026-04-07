using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanDoAnVat.Models;
using WebsiteBanDoAnVat.Data;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public async Task<IActionResult> Index(int? categoryId, int page = 1)
        {
            int pageSize = 9; // Lấy 9 món 1 trang cho mục Gợi Ý
            var viewModel = new HomeViewModel();

            // 1. Lấy danh sách danh mục để vẽ Menu
            viewModel.Categories = await _context.Categories.ToListAsync();

            // 2. Lấy 4 sản phẩm bán chạy (Mục ở trên)
            viewModel.BestSellers = await _context.MonAns
                .Where(m => m.IsAvailable)
                .OrderByDescending(m => m.Id)
                .Take(4)
                .ToListAsync();

            // 3. Xử lý phần "GỢI Ý CHO BẠN" (Cắt trang + Lọc theo Danh mục)
            var query = _context.MonAns.Where(m => m.IsAvailable).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
                ViewBag.CurrentCategoryId = categoryId.Value; // Lưu lại ID để in đậm menu
            }

            // Đếm tổng số món và tính số trang
            int totalItems = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentPage = page;

            // Cắt đúng 9 món đem ra View
            viewModel.AllProducts = await query
                .OrderByDescending(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(viewModel);
        }

        // --- QUẢN LÝ SẢN PHẨM (TRANG KHÁCH HÀNG XEM) ---
        // ĐÃ GỘP: Xử lý cả Tìm kiếm (Search), Lọc (Filter) và Phân trang vào chung 1 chỗ
        public async Task<IActionResult> SanPham(string searchString, int? categoryId, string filter, decimal? minPrice, decimal? maxPrice, int page = 1)
        {
            int pageSize = 9; // Giới hạn 9 món / 1 trang
            var query = _context.MonAns.Include(m => m.Category).Where(m => m.IsAvailable).AsQueryable();

            // Xử lý thanh tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Name.Contains(searchString));
            }

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
                ViewBag.CurrentCategoryId = categoryId.Value;
            }

            // Lọc theo khoảng giá
            if (minPrice.HasValue) query = query.Where(m => m.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(m => m.Price <= maxPrice.Value);

            // Xử lý bộ lọc
            switch (filter)
            {
                case "banchay": query = query.Where(m => m.IsBestSeller); break;
                case "moinhat": query = query.OrderByDescending(m => m.Id); break;
                case "hot": query = query.OrderByDescending(m => m.ViewCount).Where(m => m.ViewCount > 0); break;
                case "giamgia": query = query.Where(m => m.Price < 50000); break;
                default: query = query.OrderByDescending(m => m.Id); break;
            }

            // XỬ LÝ PHÂN TRANG
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await query
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            // Truyền dữ liệu ra View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Filter = filter;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SearchString = searchString;

            return View(products);
        }

        // --- QUẢN LÝ DANH MỤC ---
        public async Task<IActionResult> Category()
        {
            return View(await _context.Categories.ToListAsync());
        }

        public IActionResult CategoryCreate() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Category));
            }
            return View(category);
        }

        public async Task<IActionResult> CategoryEdit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryEdit(int id, Category category)
        {
            if (id != category.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Category));
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var category = await _context.Categories.Include(c => c.MonAns).FirstOrDefaultAsync(c => c.Id == id);
            if (category != null)
            {
                if (category.MonAns.Any())
                {
                    TempData["Error"] = "Không thể xóa danh mục này vì đang có món ăn thuộc danh mục này!";
                    return RedirectToAction(nameof(Category));
                }
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Category));
        }

        // --- QUẢN LÝ KHÁCH HÀNG & ĐƠN HÀNG (ADMIN) ---
        public async Task<IActionResult> KhachHang()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> OrderManagement()
        {
            var orders = await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.MonAn)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // --- QUẢN LÝ GIỎ HÀNG & THANH TOÁN ---
        public IActionResult Cart() => View(GetCartItems());

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _context.MonAns.Find(productId);
            if (product == null) return NotFound();
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item == null)
                cart.Add(new CartItem { ProductId = product.Id, ProductName = product.Name, Price = product.Price, Quantity = quantity, ImageUrl = product.ImageUrl });
            else
                item.Quantity += quantity;
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(string customerName, string phone, string address, string cartJson)
        {
            if (string.IsNullOrEmpty(cartJson) || cartJson == "[]")
            {
                return RedirectToAction("Cart");
            }

            var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

            var order = new Order
            {
                CustomerName = customerName,
                PhoneNumber = phone,
                Address = address,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(s => s.Price * s.Quantity),
                Status = "Chờ duyệt"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    MonAnId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }
            await _context.SaveChangesAsync();
            return View("OrderSuccess");
        }

        private List<CartItem> GetCartItems()
        {
            var sessionCart = HttpContext.Session.GetString("Cart");
            return sessionCart != null ? JsonConvert.DeserializeObject<List<CartItem>>(sessionCart) : new List<CartItem>();
        }

        // --- CÁC TRANG KHÁC ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var monAn = await _context.MonAns.Include(m => m.Category).FirstOrDefaultAsync(m => m.Id == id);
            return monAn == null ? NotFound() : View(monAn);
        }

        // BỔ SUNG: API phục vụ thanh tìm kiếm trực tiếp trên Header
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Json(new List<object>());

            var results = await _context.MonAns
                .Where(m => m.Name.Contains(keyword) && m.IsAvailable)
                .Take(10) // Giới hạn xổ ra 10 kết quả cho đẹp
                .Select(m => new { id = m.Id, name = m.Name })
                .ToListAsync();

            return Json(results);
        }

        public IActionResult GioiThieu() => View();
        public IActionResult TinTuc() => View();
        public IActionResult LienHe() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}