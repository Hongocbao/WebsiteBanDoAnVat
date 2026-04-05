using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteBanDoAnVat.Models;

namespace WebsiteBanDoAnVat.Data
{
    // Thêm <ApplicationUser> vào đây để Identity hiểu được các trường mở rộng (MaSinhVien, IsSinhVien)
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<MonAn> MonAns { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- GIỮ NGUYÊN SEED DATA CỦA BẠN ---

            // 1. Tạo dữ liệu mẫu cho Category
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Đồ Khô", Description = "Khô gà, khô bò các loại" },
                new Category { Id = 2, Name = "Bánh Tráng", Description = "Bánh tráng trộn, nướng" },
                new Category { Id = 3, Name = "Ăn Vặt Ngọt", Description = "Kẹo, bánh ngọt, trái cây sấy" }
            );

            // 2. Tạo dữ liệu mẫu cho MonAn
            builder.Entity<MonAn>().HasData(
                // 4 Món bán chạy
                new MonAn { Id = 1, Name = "Khô Bò Giòn Cay Hồng Ngự", Price = 380000, ImageUrl = "https://images.unsplash.com/photo-1621236322951-f073527a4411?w=400", IsAvailable = true, IsBestSeller = true, CategoryId = 1, StockQuantity = 50 },
                new MonAn { Id = 2, Name = "Bánh Tráng Muối Bò Premium", Price = 110000, ImageUrl = "https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=400", IsAvailable = true, IsBestSeller = true, CategoryId = 2, StockQuantity = 100 },
                new MonAn { Id = 3, Name = "Bánh Tráng Tóp Mỡ", Price = 130000, ImageUrl = "https://images.unsplash.com/photo-1621236322951-f073527a4411?w=400", IsAvailable = true, IsBestSeller = true, CategoryId = 2, StockQuantity = 80 },
                new MonAn { Id = 4, Name = "Khô Bò Vụn", Price = 320000, ImageUrl = "https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=400", IsAvailable = true, IsBestSeller = true, CategoryId = 1, StockQuantity = 30 },

                // 3 Món mới ra mắt
                new MonAn { Id = 5, Name = "Chùm Ruột Muối Tắc", Price = 120000, ImageUrl = "https://images.unsplash.com/photo-1621236322951-f073527a4411?w=400", IsAvailable = true, IsBestSeller = false, CategoryId = 3, StockQuantity = 200 },
                new MonAn { Id = 6, Name = "Kẹo Dừa Sáp Bọc Xíu", Price = 110000, ImageUrl = "https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=400", IsAvailable = true, IsBestSeller = false, CategoryId = 3, StockQuantity = 150 },
                new MonAn { Id = 7, Name = "Mực Rim Me Sấy Mè", Price = 150000, ImageUrl = "https://images.unsplash.com/photo-1621236322951-f073527a4411?w=400", IsAvailable = true, IsBestSeller = false, CategoryId = 1, StockQuantity = 40 }
            );
        }
    }
}