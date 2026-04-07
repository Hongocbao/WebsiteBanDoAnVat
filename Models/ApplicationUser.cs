using Microsoft.AspNetCore.Identity;
using System;

namespace WebsiteBanDoAnVat.Models
{
    public class ApplicationUser : IdentityUser
    {
        // === THÔNG TIN CƠ BẢN (Dùng để hiển thị thay cho 'Khách ngoài') ===
        public string? HoTen { get; set; }
        public string? DiaChi { get; set; }
        public string? AvatarUrl { get; set; } = "/images/default-avatar.png";

        // === THÔNG TIN HỆ THỐNG ===
        public DateTime NgayDangKy { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public bool IsAdmin { get; set; } = false;
        // THÊM DÒNG NÀY: Để đánh dấu tài khoản Gốc không thể bị xóa/hạ cấp
        public bool IsSuperAdmin { get; set; } = false;
    }
}