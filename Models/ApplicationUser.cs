using Microsoft.AspNetCore.Identity;

namespace WebsiteBanDoAnVat.Models
{
    public class ApplicationUser : IdentityUser
    {
        
        public string? MaSinhVien { get; set; }
        public bool IsSinhVien { get; set; }
    }
}