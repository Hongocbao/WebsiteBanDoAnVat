using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // 1. TRANG CHỦ & PHÂN TRANG
        public async Task<IActionResult> Index(int? categoryId, int page = 1)
        {
            int pageSize = 9;
            var viewModel = new HomeViewModel();
            viewModel.Categories = await _context.Categories.ToListAsync();
            viewModel.BestSellers = await _context.MonAns
                .Where(m => m.IsAvailable)
                .OrderByDescending(m => m.Id)
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
            viewModel.AllProducts = await query.OrderByDescending(m => m.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return View(viewModel);
        }

        // 2. CHI TIẾT SẢN PHẨM (Lấy kèm DanhGias)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var monAn = await _context.MonAns
                .Include(m => m.Category)
                .Include(m => m.DanhGias) // Sử dụng đúng tên Collection trong Model MonAn
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monAn == null) return NotFound();
            return View(monAn);
        }

        // 3. XỬ LÝ GỬI ĐÁNH GIÁ (Khớp Model DanhGia của bạn)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostComment(int MonAnId, int SoSao, string NoiDung, IFormFile imageFile)
        {
            // ^^^ PHẢI CÓ 'IFormFile imageFile' Ở ĐÂY ^^^

            if (string.IsNullOrWhiteSpace(NoiDung)) return RedirectToAction("Details", new { id = MonAnId });

            var danhGia = new DanhGia
            {
                MonAnId = MonAnId,
                SoSao = SoSao,
                NoiDung = NoiDung,
                NgayDang = DateTime.Now,
                UserName = User.Identity.IsAuthenticated ? User.Identity.Name : "Khách hàng"
            };

            // Đoạn code xử lý file ảnh (đã có imageFile nên sẽ hết lỗi đỏ)
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
                danhGia.HinhAnh = "/images/" + fileName;
            }

            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = MonAnId });
        }

        // 4. TRANG SẢN PHẨM & BỘ LỌC CHI TIẾT
        public async Task<IActionResult> SanPham(int? categoryId, string filter, decimal? minPrice, decimal? maxPrice, int page = 1)
        {
            int pageSize = 9;
            var query = _context.MonAns.Where(m => m.IsAvailable).AsQueryable();

            if (categoryId.HasValue) { query = query.Where(m => m.CategoryId == categoryId.Value); ViewBag.CurrentCategoryId = categoryId.Value; }
            if (minPrice.HasValue) query = query.Where(m => m.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(m => m.Price <= maxPrice.Value);

            switch (filter)
            {
                case "banchay": query = query.Where(m => m.IsBestSeller); break;
                case "moinhat": query = query.OrderByDescending(m => m.Id); break;
                case "hot": query = query.OrderByDescending(m => m.ViewCount).Where(m => m.ViewCount > 0); break;
                case "giamgia": query = query.Where(m => m.Price < 50000); break;
                default: query = query.OrderByDescending(m => m.Id); break;
            }

            int totalItems = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Filter = filter;
            return View(products);
        }

        // 5. GIỎ HÀNG
        public IActionResult Cart() => View(GetCartItems());
        [HttpGet]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            // Lấy sản phẩm từ Database để chắc chắn có Ảnh và Giá
            var product = _context.MonAns.FirstOrDefault(m => m.Id == productId);

            if (product == null) return RedirectToAction("Index");

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));

            // Sau khi thêm xong, quay lại đúng trang vừa đứng để hiện thông báo (nếu có logic hiện)
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // 6. THANH TOÁN
        [HttpPost]
        public async Task<IActionResult> Checkout(string customerName, string phone, string address, string cartJson)
        {
            if (string.IsNullOrEmpty(cartJson) || cartJson == "[]") return RedirectToAction("Cart");
            var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
            var order = new Order { CustomerName = customerName, PhoneNumber = phone, Address = address, OrderDate = DateTime.Now, TotalAmount = cartItems.Sum(s => s.Price * s.Quantity), Status = "Chờ duyệt" };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            foreach (var item in cartItems) { _context.OrderDetails.Add(new OrderDetail { OrderId = order.Id, MonAnId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.Price }); }
            await _context.SaveChangesAsync();
            return View("OrderSuccess");
        }

        private List<CartItem> GetCartItems()
        {
            var sessionCart = HttpContext.Session.GetString("Cart");
            return sessionCart != null ? JsonConvert.DeserializeObject<List<CartItem>>(sessionCart) : new List<CartItem>();
        }

        // 7. TÌM KIẾM NHANH (AJAX)
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return Json(new List<object>());
            var results = await _context.MonAns.Where(m => m.Name.Contains(keyword) && m.IsAvailable).Take(10).Select(m => new { id = m.Id, name = m.Name }).ToListAsync();
            return Json(results);
        }

        public IActionResult GioiThieu() => View();
        public IActionResult TinTuc() => View();
        public IActionResult LienHe() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}