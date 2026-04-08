using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using WebsiteBanDoAnVat.Data;
using WebsiteBanDoAnVat.Models;


namespace WebsiteBanDoAnVat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // 1. TRANG CHỦ & PHÂN TRANG
        public async Task<IActionResult> Index(int? categoryId, int page = 1)
        {
            int pageSize = 9;
            var viewModel = new HomeViewModel();
            viewModel.Categories = await _context.Categories.ToListAsync();
            viewModel.BestSellers = await _context.MonAns
                .Where(m => m.IsAvailable)
                .OrderByDescending(m => m.ViewCount)
                .Take(4)
                .ToListAsync();

            var query = _context.MonAns.Where(m => m.IsAvailable).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
                ViewBag.CurrentCategoryId = categoryId.Value;
            }

            int totalItems = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentPage = page;

            viewModel.AllProducts = await query
                .OrderByDescending(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(viewModel);
        }

        // 2. DANH SÁCH SẢN PHẨM & LỌC
        public async Task<IActionResult> SanPham(string searchString, int? categoryId, string filter, decimal? minPrice, decimal? maxPrice, int page = 1)
        {
            int pageSize = 9;
            var query = _context.MonAns.Include(m => m.Category).Where(m => m.IsAvailable).AsQueryable();

            if (!string.IsNullOrEmpty(searchString)) query = query.Where(s => s.Name.Contains(searchString));
            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
                ViewBag.CurrentCategoryId = categoryId.Value;
            }
            if (minPrice.HasValue) query = query.Where(m => m.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(m => m.Price <= maxPrice.Value);

            switch (filter)
            {
                case "banchay":
                    // Lọc món Bán chạy và xếp theo ID mới nhất
                    query = query.Where(m => m.IsBestSeller).OrderByDescending(m => m.Id);
                    break;
                case "moinhat":
                    // Xếp theo ID giảm dần (Mới thêm vào sẽ lên đầu)
                    query = query.OrderByDescending(m => m.Id);
                    break;
                case "hot":
                    // Đang HOT: Xếp theo lượt xem cao nhất giảm dần
                    query = query.OrderByDescending(m => m.ViewCount);
                    break;
                case "giatot":
                    // Giá tốt: Sắp xếp giá từ thấp đến cao (Rẻ nhất lên đầu)
                    query = query.OrderBy(m => m.Price);
                    break;
                default:
                    // Mặc định: Trộn đều (hoặc xếp theo mới nhất)
                    query = query.OrderByDescending(m => m.Id);
                    break;
            }

            int totalItems = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Filter = filter;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SearchString = searchString;

            return View(products);
        }

        // 3. CHI TIẾT SẢN PHẨM
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var monAn = await _context.MonAns
                .Include(m => m.Category)
                .Include(m => m.DanhGias)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monAn == null) return NotFound();
            return View(monAn);
        }

        // 4. XỬ LÝ GỬI ĐÁNH GIÁ
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostComment(int MonAnId, int SoSao, string NoiDung, IFormFile imageFile)
        {
            if (string.IsNullOrWhiteSpace(NoiDung)) return RedirectToAction("Details", new { id = MonAnId });

            var danhGia = new DanhGia
            {
                MonAnId = MonAnId,
                SoSao = SoSao,
                NoiDung = NoiDung,
                NgayDang = DateTime.Now,
                UserName = User.Identity?.IsAuthenticated == true ? (User.Identity.Name ?? "Khách hàng") : "Khách hàng"
            };

            if (imageFile != null && imageFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/reviews");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                danhGia.HinhAnh = "/images/reviews/" + fileName;
            }

            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = MonAnId });
        }

        // 5. GIỎ HÀNG & THANH TOÁN (Chỉ giữ lại View, Data do JS gửi lên)
        public IActionResult Cart() => View();

        [HttpPost]
        public async Task<IActionResult> Checkout(string customerName, string phone, string address, string cartJson)
        {
            if (string.IsNullOrEmpty(cartJson) || cartJson == "[]") return RedirectToAction("Cart");

            var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cartJson!);
            // LẤY ID NGƯỜI DÙNG ĐANG ĐĂNG NHẬP
            var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;

            var order = new Order
            {
                UserId = userId,
                CustomerName = customerName,
                PhoneNumber = phone,
                Address = address,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems!.Sum(s => s.Price * s.Quantity),
                Status = "Chờ duyệt"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // ĐÃ SỬA LỖI: Thêm chi tiết món ăn vào Database
            foreach (var item in cartItems!)
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

        // 6. QUẢN LÝ ĐƠN HÀNG CỦA KHÁCH (User Thường)
        [Authorize]
        public async Task<IActionResult> OrderManagement()
        {
            var orders = await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(d => d.MonAn)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // 7. TÌM KIẾM NHANH (AJAX)
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return Json(new List<object>());
            var results = await _context.MonAns.Where(m => m.Name.Contains(keyword) && m.IsAvailable).Take(10).Select(m => new { id = m.Id, name = m.Name }).ToListAsync();
            return Json(results);
        }
        // --- 8. QUẢN LÝ DANH MỤC (ADMIN) ---
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

        // --- CÁC TRANG TĨNH ---
        public IActionResult GioiThieu() => View();
        public IActionResult TinTuc() => View();
        public IActionResult LienHe() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}