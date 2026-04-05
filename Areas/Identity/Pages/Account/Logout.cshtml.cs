// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebsiteBanDoAnVat.Models; // Thêm dòng này để nhận diện ApplicationUser

namespace WebsiteBanDoAnVat.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        // Đổi IdentityUser thành ApplicationUser ở đây
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Quay về trang chủ sau khi đăng xuất thành công
                return RedirectToPage("/Index");
            }
        }

        // Thêm phương thức OnGet để hỗ trợ đăng xuất trực tiếp qua link nếu cần
        public async Task<IActionResult> OnGet(string returnUrl = null)
        {
            return await OnPost(returnUrl);
        }
    }
}