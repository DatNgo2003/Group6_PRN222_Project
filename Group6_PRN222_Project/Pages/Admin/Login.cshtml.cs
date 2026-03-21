using Group6_PRN222_Project.Auth;
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

        // Các role được phép dùng trang này
        private static readonly HashSet<string> AllowedRoles = new()
        {
            "Admin", "Organizer",
            "Staff(Security)", "Staff(MKT)", "Staff(Logistics)"
        };

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

            // ── Chặn Participant: yêu cầu dùng trang login thường ─────
            var roleName = user.Role?.RoleName ?? "";
            if (!AllowedRoles.Contains(roleName))
            {
                ErrorMessage = "Tài khoản của bạn không có quyền truy cập hệ thống nội bộ. Vui lòng sử dụng trang đăng nhập thường.";
                return Page();
            }

            // ── Tạo JWT + lưu cookie & session ────────────────────────
            var token = _jwt.GenerateToken(user);
            SetAuthCookieAndSession(token, user, RememberMe);

            // ── Ghi audit log ──────────────────────────────────────────
            _db.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId = user.UserId,
                Action = $"LOGIN_STAFF_{roleName.ToUpper().Replace("(", "").Replace(")", "").Replace(" ", "_")}",
                TableName = "Users",
                ActionTime = DateTime.Now
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Chào mừng {user.FullName ?? user.Username}!";

            // ── Redirect theo từng role cụ thể ────────────────────────
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            return roleName switch
            {
                "Admin" => RedirectToPage("/Admin/AdminDashboard"),
                "Organizer" => RedirectToPage("/Organizer/OrganizerDashboard"),
                "Staff(Security)" => RedirectToPage("/Staff/StaffDashboard"),
                "Staff(MKT)" => RedirectToPage("/Staff/StaffDashboard"),
                "Staff(Logistics)" => RedirectToPage("/Staff/StaffDashboard"),
                _ => RedirectToPage("/Index")
            };
        }

        private void SetAuthCookieAndSession(string token, User user, bool rememberMe)
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
            HttpContext.Session.SetString("username", user.Username);
            HttpContext.Session.SetString("fullname", user.FullName ?? user.Username);
            HttpContext.Session.SetInt32("userid", user.UserId);
            HttpContext.Session.SetString("role", user.Role?.RoleName ?? "");
        }
    }
}
