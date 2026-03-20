using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_PRN222.Helpers;
using Group6_PRN222_Project.Models;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.Departments
{
    public class CreateModel : PageModel
    {
        private readonly IDepartmentService _deptSvc;
        private readonly IAuditLogService   _audit;

        public CreateModel(IDepartmentService deptSvc, IAuditLogService audit)
        {
            _deptSvc = deptSvc;
            _audit   = audit;
        }

        [BindProperty] public Department InputDept { get; set; } = new();

        public IActionResult OnGet()
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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToPage("/Auth/Login");
#else
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Auth/Login");
#endif

            if (await _deptSvc.NameExistsAsync(InputDept.DepartmentName ?? ""))
                ModelState.AddModelError("InputDept.DepartmentName", "Tên phòng ban đã tồn tại.");

            if (!ModelState.IsValid) return Page();

            var created = await _deptSvc.CreateAsync(InputDept);
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            _audit.Log(actorId, $"CREATE Department '{created.DepartmentName}' (ID={created.DepartmentId})", "Departments");

            TempData["SuccessMessage"] = $"Đã tạo phòng ban '{created.DepartmentName}' thành công.";
            return RedirectToPage("Index");
        }
    }
}
