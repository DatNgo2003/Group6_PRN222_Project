using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Helpers;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Group6_PRN222_Project.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        private readonly JwtService _jwt;

        public LoginModel(ProjectPrn222Context db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập username")]
        public string Username { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = "";

        [BindProperty]
        public bool RememberMe { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == Username);

            if (user == null || user.PasswordHash != Password)
            {
                ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return Page();
            }

            var roleName = user.Role?.RoleName ?? "";

            // ── Chặn role nội bộ, chỉ cho Participant đăng nhập ──────
            var internalRoles = new[]
            {
                InternalRoleResolver.Admin,
                InternalRoleResolver.Organizer,
                InternalRoleResolver.StaffSecurity,
                InternalRoleResolver.StaffMkt,
                InternalRoleResolver.StaffLogistics
            };

            if (internalRoles.Any(r => string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Tài khoản nội bộ vui lòng sử dụng trang đăng nhập nhân viên.";
                return Page();
            }

            if (!string.Equals(roleName, InternalRoleResolver.Participant, StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Tài khoản không hợp lệ.";
                return Page();
            }

            // ── Tạo JWT với roleName đúng ─────────────────────────────
            var token = _jwt.GenerateToken(user, roleName);
            SetAuthCookieAndSession(token, user, RememberMe);

            _db.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId = user.UserId,
                Action = "LOGIN_PARTICIPANT",
                TableName = "Users",
                ActionTime = DateTime.Now
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Chào mừng {user.FullName ?? user.Username}!";

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            return RedirectToPage("/Index");
        }

        private void SetAuthCookieAndSession(string token, User user, bool rememberMe)
        {
            // ── Lưu JWT vào cookie ─────────────────────────────────────
            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = rememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(8)
            });

            // ── Lưu thông tin cơ bản vào Session để dùng trong UI ─────
            HttpContext.Session.SetString("auth_token", token);
            HttpContext.Session.SetString("username", user.Username);
            HttpContext.Session.SetString("fullname", user.FullName ?? user.Username);
            HttpContext.Session.SetString("email", user.Email ?? "");
            HttpContext.Session.SetString("role", user.Role?.RoleName ?? "");
            HttpContext.Session.SetInt32("userid", user.UserId);
        }
    }
}