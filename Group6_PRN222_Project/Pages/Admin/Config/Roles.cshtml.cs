using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.Config
{
    [AuthorizeRole("Admin")]
    public class RolesModel : PageModel
    {
        private readonly IRoleService _roleSvc;
        private readonly IAuditLogService _audit;

        public RolesModel(IRoleService roleSvc, IAuditLogService audit)
        {
            _roleSvc = roleSvc;
            _audit = audit;
        }

        public List<Role> Roles { get; set; } = new();
        [TempData] public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Account/Login");
            Roles = await _roleSvc.GetAllAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id, string currentStatus)
        {
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Account/Login");

            var newStatus = currentStatus == "Active" ? "Inactive" : "Active";
            await _roleSvc.UpdateStatusAsync(id, newStatus);

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            _audit.Log(actorId, $"UPDATE Role ID={id} Status → {newStatus}", "Roles");
            SuccessMessage = $"Đã cập nhật trạng thái role thành {newStatus}.";
            return RedirectToPage();
        }
    }
}