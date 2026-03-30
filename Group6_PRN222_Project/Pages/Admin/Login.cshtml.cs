using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Helpers;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using AppTask = System.Threading.Tasks.Task;

namespace Group6_PRN222_Project.Pages.Admin
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

            // ── Tìm user ───────────────────────────────────────────────
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == Username);

            if (user == null || user.PasswordHash != Password)
            {
                ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return Page();
            }

            // ── Dùng InternalRoleResolver map tên role từ DB ───────────
            if (!InternalRoleResolver.TryResolveAppRole(user.Role?.RoleName, out var appRole))
            {
                ErrorMessage = "Tài khoản của bạn không có quyền truy cập hệ thống nội bộ. Vui lòng sử dụng trang đăng nhập thường.";
                return Page();
            }

            // ── Tạo JWT + lưu cookie & session ────────────────────────
            var token = _jwt.GenerateToken(user, appRole);
            SetAuthCookieAndSession(token, user, appRole, RememberMe);

            // ── Ghi audit log ──────────────────────────────────────────
            _db.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId = user.UserId,
                Action = $"LOGIN_{appRole.ToUpper().Replace("(", "").Replace(")", "").Replace(" ", "_")}",
                TableName = "Users",
                ActionTime = DateTime.Now
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Chào mừng {user.FullName ?? user.Username}!";

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            // ── Redirect theo appRole đã resolve ──────────────────────
            return appRole switch
            {
                InternalRoleResolver.Admin => RedirectToPage("/Admin/Reports/RevenueReport"),
                InternalRoleResolver.Organizer => RedirectToPage("/Organizer/Dashboard"),
                InternalRoleResolver.StaffSecurity => RedirectToPage("/StaffSecurity/Index"),
                InternalRoleResolver.StaffMkt => RedirectToPage("/StaffMKT/Index"),
                InternalRoleResolver.StaffLogistics => RedirectToPage("/StaffLogistics/Index"),
                _ => RedirectToPage("/Index")
            };
        }

        private void SetAuthCookieAndSession(string token, User user, string appRole, bool rememberMe)
        {
            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = rememberMe
                              ? DateTimeOffset.UtcNow.AddDays(7)
                              : DateTimeOffset.UtcNow.AddHours(8)
            });
            // ── Lưu token ──────────────────────────────────────────────
            HttpContext.Session.SetString("auth_token", token);

            // ── Lưu cả lowercase (dùng nguyên bản) và PascalCase ──────
            // PascalCase: SessionHelper.cs dùng "UserID","UserName","FullName","Role"
            // Lowercase:  các trang khác dùng "userid","username","fullname","role"
            HttpContext.Session.SetInt32("userid",  user.UserId);
            HttpContext.Session.SetInt32("UserID",  user.UserId);

            HttpContext.Session.SetString("username",  user.Username);
            HttpContext.Session.SetString("UserName",  user.Username);

            HttpContext.Session.SetString("fullname",  user.FullName ?? user.Username);
            HttpContext.Session.SetString("FullName",  user.FullName ?? user.Username);

            HttpContext.Session.SetString("email",    user.Email ?? "");

            // Lưu appRole (đã chuẩn hoá) thay vì role raw từ DB
            HttpContext.Session.SetString("role", appRole);
            HttpContext.Session.SetString("Role", appRole);
        }
    }
}