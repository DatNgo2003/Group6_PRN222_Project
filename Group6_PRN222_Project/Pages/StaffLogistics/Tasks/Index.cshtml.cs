using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.StaffLogistics.Tasks
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        private readonly IAuditLogService _audit;

        public IndexModel(ProjectPrn222Context db, IAuditLogService audit)
        {
            _db = db;
            _audit = audit;
        }

        public List<Group6_PRN222_Project.Models.Task> Tasks { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", RoleConstants.StaffLogistics);
                HttpContext.Session.SetString("FullName", "Dev Logistics");
                HttpContext.Session.SetString("UserName", "logistics");
            }
#else
            var isLogged = SessionHelper.IsLoggedIn(HttpContext.Session);
            var role = SessionHelper.GetRole(HttpContext.Session);
            if (!isLogged || role != RoleConstants.StaffLogistics)
                return RedirectToPage("/Auth/Login");
#endif

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;

            Tasks = await _db.Tasks
                .AsNoTracking()
                .Include(t => t.Event)
                .Where(t => t.AssignedTo == actorId)
                .OrderBy(t => t.Deadline)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int taskId, string status)
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToPage("/Auth/Login");
#else
            var isLogged = SessionHelper.IsLoggedIn(HttpContext.Session);
            var role = SessionHelper.GetRole(HttpContext.Session);
            if (!isLogged || role != RoleConstants.StaffLogistics)
                return RedirectToPage("/Auth/Login");
#endif

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;

            status = status?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(status))
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
                return RedirectToPage();
            }

            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId && t.AssignedTo == actorId);
            if (task == null)
            {
                TempData["Error"] = "Không tìm thấy task hoặc bạn không có quyền cập nhật.";
                return RedirectToPage();
            }

            task.Status = status;
            await _db.SaveChangesAsync();

            _audit.Log(actorId, $"UPDATE Task ID={taskId} Status='{status}'", "Tasks");

            TempData["Success"] = "Cập nhật trạng thái task thành công.";
            return RedirectToPage();
        }
    }
}

