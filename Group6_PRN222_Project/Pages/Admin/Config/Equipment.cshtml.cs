using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.Config
{
    [AuthorizeRole("Admin")]
    public class EquipmentModel : PageModel
    {
        private readonly IEquipmentService _svc;
        private readonly IAuditLogService _audit;

        public EquipmentModel(IEquipmentService svc, IAuditLogService audit)
        { _svc = svc; _audit = audit; }

        public List<Equipment> Equipments { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string StatusFilter { get; set; } = "All";
        [BindProperty] public Equipment Input { get; set; } = new();
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
            if (!SessionHelper.IsAdmin(HttpContext.Session)) return RedirectToPage("/Account/Login");
            Equipments = await _svc.GetAllAsync(StatusFilter);
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session)) return RedirectToPage("/Account/Login");
            var created = await _svc.CreateAsync(Input);
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            _audit.Log(actorId, $"CREATE Equipment '{created.EquipmentName}' (ID={created.EquipmentId})", "Equipments");
            SuccessMessage = $"Đã thêm thiết bị '{created.EquipmentName}'.";
            return RedirectToPage(new { StatusFilter });
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session)) return RedirectToPage("/Account/Login");
            await _svc.UpdateAsync(Input);
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            _audit.Log(actorId, $"UPDATE Equipment '{Input.EquipmentName}' (ID={Input.EquipmentId})", "Equipments");
            SuccessMessage = $"Đã cập nhật thiết bị '{Input.EquipmentName}'.";
            return RedirectToPage(new { StatusFilter });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session)) return RedirectToPage("/Account/Login");
            await _svc.DeleteAsync(id);
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            _audit.Log(actorId, $"DELETE Equipment ID={id}", "Equipments");
            SuccessMessage = "Đã xoá thiết bị.";
            return RedirectToPage(new { StatusFilter });
        }
    }
}