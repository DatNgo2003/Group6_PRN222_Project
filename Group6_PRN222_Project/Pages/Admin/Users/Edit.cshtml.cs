using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Group6_PRN222_Project.Models;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.Users
{
    public class EditModel : PageModel
    {
        private readonly IUserService     _userSvc;
        private readonly IAuditLogService _audit;
        private readonly ProjectPrn222Context _db;

        public EditModel(IUserService userSvc, IAuditLogService audit, ProjectPrn222Context db)
        {
            _userSvc = userSvc;
            _audit   = audit;
            _db      = db;
        }

        [BindProperty] public User   InputUser      { get; set; } = new();
        [BindProperty] public string NewPassword    { get; set; } = string.Empty;
        [BindProperty] public string ConfirmPassword{ get; set; } = string.Empty;

        public SelectList RoleList       { get; set; } = default!;
        public SelectList DepartmentList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Auth/Login");

            var user = await _userSvc.GetByIdAsync(id);
            if (user == null) return NotFound();

            InputUser = user;
            await LoadSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Auth/Login");

            // Validate new password nếu có nhập
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword.Length < 6)
                    ModelState.AddModelError(nameof(NewPassword), "Mật khẩu phải ít nhất 6 ký tự.");
                else if (NewPassword != ConfirmPassword)
                    ModelState.AddModelError(nameof(ConfirmPassword), "Xác nhận mật khẩu không khớp.");
            }

            // Validate duplicate username (trừ user hiện tại)
            if (await _userSvc.UsernameExistsAsync(InputUser.Username, InputUser.UserId))
                ModelState.AddModelError("InputUser.Username", "Username đã tồn tại.");

            ModelState.Remove("InputUser.PasswordHash");
            ModelState.Remove(nameof(NewPassword));
            ModelState.Remove(nameof(ConfirmPassword));

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            // Giữ nguyên PasswordHash cũ nếu không đổi mật khẩu
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                var existing = await _db.Users.AsNoTracking()
                                              .FirstOrDefaultAsync(u => u.UserId == InputUser.UserId);
                InputUser.PasswordHash = existing?.PasswordHash ?? InputUser.PasswordHash;
            }

            await _userSvc.UpdateAsync(InputUser, string.IsNullOrWhiteSpace(NewPassword) ? null : NewPassword);

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            _audit.Log(actorId, $"UPDATE User '{InputUser.Username}' (ID={InputUser.UserId})", "Users");

            TempData["SuccessMessage"] = $"Đã cập nhật tài khoản '{InputUser.Username}' thành công.";
            return RedirectToPage("Index");
        }

        private async Task LoadSelectListsAsync()
        {
            var roles = await _db.Roles.OrderBy(r => r.RoleId).ToListAsync();
            var depts = await _db.Departments.OrderBy(d => d.DepartmentId).ToListAsync();
            RoleList       = new SelectList(roles, "RoleId", "RoleName", InputUser.RoleId);
            DepartmentList = new SelectList(depts, "DepartmentId", "DepartmentName", InputUser.DepartmentId);
        }
    }
}
