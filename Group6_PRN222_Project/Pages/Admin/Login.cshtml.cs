using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Helpers;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
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

            // ── Chuẩn hóa role DB → role app (JWT / AuthorizeRole) ────
            var dbRoleName = user.Role?.RoleName ?? "";
            if (!InternalRoleResolver.TryResolveAppRole(dbRoleName, out var appRole))
            {
                ErrorMessage = "Tài khoản của bạn không có quyền truy cập hệ thống nội bộ. Role trong DB \"" + dbRoleName + "\" chưa được map (cần Admin, Organizer, Staff, Staff(Security), Marketing, Logistics…).";
                return Page();
            }

            // ── Tạo JWT + lưu cookie & session ────────────────────────
            var token = _jwt.GenerateToken(user, appRole);
            SetAuthCookieAndSession(token, user, appRole, RememberMe);

            // ── Ghi audit log ──────────────────────────────────────────
            _db.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId = user.UserId,
                Action = $"LOGIN_STAFF_{appRole.ToUpper().Replace("(", "").Replace(")", "").Replace(" ", "_")}",
                TableName = "Users",
                ActionTime = DateTime.Now
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Chào mừng {user.FullName ?? user.Username}!";

            // ── Redirect theo từng role cụ thể ────────────────────────
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            return appRole switch
            {
                InternalRoleResolver.Admin => RedirectToPage("/Admin/Reports/RevenueReport"),
                InternalRoleResolver.Organizer => RedirectToPage("/Organizer/Dashboard"),
                InternalRoleResolver.StaffSecurity => RedirectToPage("/StaffSecurity/Index"),
                InternalRoleResolver.StaffMkt => RedirectToPage("/Staff/StaffDashboard"),
                InternalRoleResolver.StaffLogistics => RedirectToPage("/Staff/StaffDashboard"),
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
            HttpContext.Session.SetString("auth_token", token);
            // Dùng SessionHelper để ghi đúng PascalCase keys mà toàn bộ app đọc
            SessionHelper.SetUser(HttpContext.Session, user.UserId, user.Username, user.FullName ?? user.Username, appRole);
        }

    }
}
