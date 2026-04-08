using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Threading.Tasks;
using WebsiteBanDoAnVat.Models;

namespace WebsiteBanDoAnVat.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Biến này để truyền thông báo ra ngoài màn hình HTML
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Không tìm thấy tài khoản có ID = '{userId}'.");
            }

            // BẮT BUỘC CÓ: Giải mã lại cái code (Vì lúc gửi mail mình có mã hóa nó cho an toàn)
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            // Chốt hạ: Tự động đổi False thành True trong Database
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                StatusMessage = "Xác nhận Email thành công! Bây giờ bạn có thể dùng tính năng Quên mật khẩu hoặc Đăng nhập.";
            }
            else
            {
                StatusMessage = "Mã xác nhận không hợp lệ hoặc link này đã hết hạn.";
            }

            return Page();
        }
    }
}