using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Group6_PRN222_Project.Models;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.Departments
{
    public class IndexModel : PageModel
    {
        private readonly IDepartmentService  _deptSvc;
        private readonly IAuditLogService    _audit;
        private readonly ProjectPrn222Context _db;

        public IndexModel(IDepartmentService deptSvc, IAuditLogService audit, ProjectPrn222Context db)
        {
            _deptSvc = deptSvc;
            _audit   = audit;
            _db      = db;
        }

        public List<DepartmentViewModel> Departments { get; set; } = new();

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage   { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID",   1);
                HttpContext.Session.SetString("Role",     "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
#else
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Auth/Login");
#endif

            var depts = await _deptSvc.GetAllAsync();

            // Đếm số user trong mỗi department
            var userCounts = await _db.Users
                .Where(u => u.DepartmentId != null)
                .GroupBy(u => u.DepartmentId)
                .Select(g => new { DeptId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DeptId!.Value, x => x.Count);

            Departments = depts.Select(d => new DepartmentViewModel
            {
                Department = d,
                UserCount  = userCounts.GetValueOrDefault(d.DepartmentId, 0)
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToPage("/Auth/Login");
#else
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Auth/Login");
#endif

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            var success = await _deptSvc.DeleteAsync(id);

            if (!success)
            {
                ErrorMessage = "Không thể xoá — phòng ban này vẫn còn người dùng.";
            }
            else
            {
                _audit.Log(actorId, $"DELETE Department ID={id}", "Departments");
                SuccessMessage = "Đã xoá phòng ban thành công.";
            }

            return RedirectToPage();
        }
    }

    // ViewModel gộp Department + số user
    public class DepartmentViewModel
    {
        public Department Department { get; set; } = default!;
        public int        UserCount  { get; set; }
    }
}
