using Microsoft.AspNetCore.Identity;

namespace WebsiteBanDoAnVat.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Giữ nguyên của bạn
        public string? MaSinhVien { get; set; }
        public bool IsSinhVien { get; set; }

        // Thêm mới để phục vụ logic quản lý và hiển thị
        public string? HoTen { get; set; } // Để hiện tên thay vì dùng Email
        public DateTime NgayDangKy { get; set; } = DateTime.Now; // Để sắp xếp khách mới lên đầu
        public bool IsActive { get; set; } = true; // Phục vụ logic Soft Delete
    }
}