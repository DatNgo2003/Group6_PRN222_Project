using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Group6_PRN222_Project.Models;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.Users
{
    public class CreateModel : PageModel
    {
        private readonly IUserService     _userSvc;
        private readonly IAuditLogService _audit;
        private readonly ProjectPrn222Context _db;

        public CreateModel(IUserService userSvc, IAuditLogService audit, ProjectPrn222Context db)
        {
            _userSvc = userSvc;
            _audit   = audit;
            _db      = db;
        }

        [BindProperty] public User   InputUser    { get; set; } = new();
        [BindProperty] public string PlainPassword { get; set; } = string.Empty;
        [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;

        public SelectList RoleList       { get; set; } = default!;
        public SelectList DepartmentList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Auth/Login");

            await LoadSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Auth/Login");

            // Validate password
            if (string.IsNullOrWhiteSpace(PlainPassword))
                ModelState.AddModelError(nameof(PlainPassword), "Mật khẩu không được để trống.");
            else if (PlainPassword.Length < 6)
                ModelState.AddModelError(nameof(PlainPassword), "Mật khẩu phải ít nhất 6 ký tự.");
            else if (PlainPassword != ConfirmPassword)
                ModelState.AddModelError(nameof(ConfirmPassword), "Xác nhận mật khẩu không khớp.");

            // Validate duplicate username
            if (await _userSvc.UsernameExistsAsync(InputUser.Username))
                ModelState.AddModelError("InputUser.Username", "Username đã tồn tại.");

            // Chỉ validate các field cần thiết
            ModelState.Remove("InputUser.PasswordHash");

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            var created = await _userSvc.CreateAsync(InputUser, PlainPassword);

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            _audit.Log(actorId, $"CREATE User '{created.Username}' (ID={created.UserId})", "Users");

            TempData["SuccessMessage"] = $"Đã tạo tài khoản '{created.Username}' thành công.";
            return RedirectToPage("Index");
        }

        private async Task LoadSelectListsAsync()
        {
            var roles = await _db.Roles.OrderBy(r => r.RoleId).ToListAsync();
            var depts = await _db.Departments.OrderBy(d => d.DepartmentId).ToListAsync();
            RoleList       = new SelectList(roles, "RoleId", "RoleName");
            DepartmentList = new SelectList(depts, "DepartmentId", "DepartmentName");
        }
    }
}
