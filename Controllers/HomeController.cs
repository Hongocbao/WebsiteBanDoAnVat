using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanDoAnVat.Models;
using WebsiteBanDoAnVat.Data;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

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

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            // 1. Lấy danh sách danh mục
            viewModel.Categories = await _context.Categories.ToListAsync();

            // 2. Lấy sản phẩm bán chạy (Ví dụ: lấy 4 món có giá cao nhất hoặc mới nhất)
            // Nếu Bảo có cột IsHot hoặc IsBestSeller trong DB thì dùng .Where(m => m.IsHot)
            viewModel.BestSellers = await _context.MonAns
                .OrderByDescending(m => m.Id) // Lấy món mới nhất
                .Take(4)
                .ToListAsync();

            // 3. Lấy toàn bộ sản phẩm cho mục phía dưới
            viewModel.AllProducts = await _context.MonAns.ToListAsync();

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var monAn = await _context.MonAns.Include(m => m.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (monAn == null) return NotFound();
            return View(monAn);
        }

        // Thêm tham số minPrice và maxPrice vào đây
        public async Task<IActionResult> SanPham(string filter, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.MonAns.Where(m => m.IsAvailable).AsQueryable();

            // Lọc theo khoảng giá nếu có dữ liệu truyền vào
            if (minPrice.HasValue)
            {
                query = query.Where(m => m.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(m => m.Price <= maxPrice.Value);
            }

            switch (filter)
            {
                case "banchay":
                    query = query.Where(m => m.IsBestSeller);
                    break;
                case "moinhat":
                    query = query.OrderByDescending(m => m.Id);
                    break;
                case "hot":
                    query = query.OrderByDescending(m => m.ViewCount).Where(m => m.ViewCount > 0);
                    break;
                case "giamgia":
                    query = query.Where(m => m.Price < 50000);
                    break;
                default:
                    break;
            }

            return View(await query.ToListAsync());
        }

        public IActionResult Cart()
        {
            // Lấy giỏ hàng từ Session hoặc cứ tạo một danh sách trống để tránh lỗi Model null
            var cart = GetCartItems();
            return View(cart);
        }
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _context.MonAns.FirstOrDefault(p => p.Id == productId);
            if (product == null) return NotFound();

            var cart = GetCartItems();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (cartItem == null)
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
                cartItem.Quantity += quantity;
            }

            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(string customerName, string phone, string address, string cartJson)
        {
            // 1. Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(cartJson) || cartJson == "[]")
            {
                return RedirectToAction("Cart");
            }

            // 2. Giải mã JSON từ JavaScript gửi lên thành danh sách đối tượng C#
            var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

            // 3. Tạo mới một Đơn hàng (Order)
            var order = new Order
            {
                CustomerName = customerName, // "Khách vãng lai" từ form ẩn
                PhoneNumber = phone,
                Address = address,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(s => s.Price * s.Quantity),
                Status = "Chờ duyệt"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Lưu để lấy được OrderId tự tăng

            // 4. Lưu chi tiết từng món ăn vào bảng OrderDetails
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    MonAnId = item.ProductId, // Phải khớp với ID món ăn trong DB
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync();

            // 5. Trình diễn trang thông báo thành công
            return View("OrderSuccess");
        }

        private List<CartItem> GetCartItems()
        {
            var sessionCart = HttpContext.Session.GetString("Cart");
            if (sessionCart != null) return JsonConvert.DeserializeObject<List<CartItem>>(sessionCart);
            return new List<CartItem>();
        }

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