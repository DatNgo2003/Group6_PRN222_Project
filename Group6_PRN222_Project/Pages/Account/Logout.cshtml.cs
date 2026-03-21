using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AppTask = System.Threading.Tasks.Task;

namespace Group6_PRN222_Project.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public LogoutModel(ProjectPrn222Context db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // ── Ghi audit log trước khi xóa session ──────────────────
            var userIdStr = HttpContext.Session.GetString("userid");
            if (int.TryParse(userIdStr, out int userId))
            {
                _db.SystemAuditLogs.Add(new SystemAuditLog
                {
                    UserId = userId,
                    Action = "LOGOUT",
                    TableName = "Users",
                    ActionTime = DateTime.Now
                });
                await _db.SaveChangesAsync();
            }

            // ── Xóa cookie và session ─────────────────────────────────
            Response.Cookies.Delete("auth_token");
            HttpContext.Session.Clear();

            TempData["Success"] = "Đã đăng xuất thành công.";
            return RedirectToPage("/Account/Login");
        }

        // GET: redirect về login nếu ai đó vào /Account/Logout trực tiếp
        public IActionResult OnGet() => RedirectToPage("/Account/Login");
    }
}
