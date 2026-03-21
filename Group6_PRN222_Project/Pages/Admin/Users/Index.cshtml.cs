using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_PRN222.Helpers;
using Group6_PRN222_Project.Models;
using Project_PRN222.Services;
using Group6_PRN222_Project.Auth;
namespace Project_PRN222.Pages.Admin.Users
{
    [AuthorizeRole("Admin")]
    public class IndexModel : PageModel
    {
        private readonly IUserService _userSvc;
        private readonly IAuditLogService _audit;

        public IndexModel(IUserService userSvc, IAuditLogService audit)
        {
            _userSvc = userSvc;
            _audit = audit;
        }

        public List<User> Users { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterRoleId { get; set; }

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Tự động login Admin khi chạy debug — xoá khi build Release
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }

            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Auth/Login");

            var all = await _userSvc.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
                all = all.Where(u =>
                    u.Username.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (u.FullName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Email?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();

            if (FilterRoleId.HasValue)
                all = all.Where(u => u.RoleId == FilterRoleId).ToList();

            Users = all;
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToPage("/Auth/Login");
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Auth/Login");

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;

            if (id == actorId)
            {
                ErrorMessage = "Không thể xoá tài khoản đang đăng nhập.";
                return RedirectToPage();
            }

            await _userSvc.DeleteAsync(id);
            _audit.Log(actorId, $"DELETE User ID={id}", "Users");

            SuccessMessage = "Đã xoá người dùng thành công.";
            return RedirectToPage();
        }
    }
}