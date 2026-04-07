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

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();
            viewModel.Categories = await _context.Categories.ToListAsync();
            viewModel.BestSellers = await _context.MonAns
                .OrderByDescending(m => m.Id)
                .Take(4)
                .ToListAsync();
            viewModel.AllProducts = await _context.MonAns.ToListAsync();
            return View(viewModel);
        }

        // --- QUẢN LÝ SẢN PHẨM (TRANG KHÁCH HÀNG XEM) ---
        public async Task<IActionResult> SanPham(string searchString, int? categoryId)
        {
            var monAns = _context.MonAns.Include(m => m.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                monAns = monAns.Where(s => s.Name.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                monAns = monAns.Where(x => x.CategoryId == categoryId);
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(await monAns.ToListAsync());
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
            if (string.IsNullOrEmpty(cartJson) || cartJson == "[]") return RedirectToAction("Cart");
            var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
            var order = new Order { CustomerName = customerName, PhoneNumber = phone, Address = address, OrderDate = DateTime.Now, TotalAmount = cartItems.Sum(s => s.Price * s.Quantity), Status = "Chờ duyệt" };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            foreach (var item in cartItems)
            {
                _context.OrderDetails.Add(new OrderDetail { OrderId = order.Id, MonAnId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.Price });
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

        public IActionResult GioiThieu() => View();
        public IActionResult TinTuc() => View();
        public IActionResult LienHe() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}