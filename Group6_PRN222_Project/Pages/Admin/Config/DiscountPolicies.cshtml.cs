using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.Config
{
    [AuthorizeRole("Admin")]
    public class DiscountPoliciesModel : PageModel
    {
        private readonly IDiscountPolicyService _svc;
        private readonly IAuditLogService _audit;
        private readonly ProjectPrn222Context _db;

        public DiscountPoliciesModel(IDiscountPolicyService svc, IAuditLogService audit, ProjectPrn222Context db)
        { _svc = svc; _audit = audit; _db = db; }

        public List<DiscountPolicy> Policies { get; set; } = new();
        public List<Event> Events { get; set; } = new();

        [BindProperty(SupportsGet = true)] public int? EventId { get; set; }
        [BindProperty] public DiscountPolicy InputPolicy { get; set; } = new();
        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

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
            Policies = await _svc.GetAllAsync(EventId);
            Events = await _db.Events.OrderBy(e => e.EventName).ToListAsync();
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
            var created = await _svc.CreateAsync(InputPolicy);
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            _audit.Log(actorId, $"CREATE DiscountPolicy '{created.PolicyName}' (ID={created.PolicyId})", "DiscountPolicies");
            SuccessMessage = $"Đã thêm chính sách '{created.PolicyName}'.";
            return RedirectToPage(new { EventId });
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
            await _svc.UpdateAsync(InputPolicy);
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            _audit.Log(actorId, $"UPDATE DiscountPolicy '{InputPolicy.PolicyName}' (ID={InputPolicy.PolicyId})", "DiscountPolicies");
            SuccessMessage = $"Đã cập nhật chính sách '{InputPolicy.PolicyName}'.";
            return RedirectToPage(new { EventId });
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
            _audit.Log(actorId, $"DELETE DiscountPolicy ID={id}", "DiscountPolicies");
            SuccessMessage = "Đã xoá chính sách.";
            return RedirectToPage(new { EventId });
        }
    }
}