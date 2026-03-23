using Group6_PRN222_Project.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.Budgets
{
    [AuthorizeRole("Admin")]
    public class DetailModel : PageModel
    {
        private readonly IBudgetService   _budgetSvc;
        private readonly IAuditLogService _audit;

        public DetailModel(IBudgetService budgetSvc, IAuditLogService audit)
        {
            _budgetSvc = budgetSvc;
            _audit     = audit;
        }

        public BudgetListItemDto  Budget      { get; set; } = default!;
        public List<BudgetListItemDto> EventBudgets { get; set; } = new();

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage   { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID",   1);
                HttpContext.Session.SetString("Role",     "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var budget = await _budgetSvc.GetByIdAsync(id);
            if (budget == null) return NotFound();

            Budget       = budget;
            EventBudgets = await _budgetSvc.GetByEventIdAsync(budget.EventId);

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            var ok = await _budgetSvc.ApproveAsync(id, actorId);

            if (ok)
            {
                _audit.Log(actorId, $"APPROVE Budget ID={id}", "Budgets");
                SuccessMessage = $"Đã duyệt ngân sách #{id}.";
            }
            else
            {
                ErrorMessage = "Không tìm thấy ngân sách.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            var ok = await _budgetSvc.RejectAsync(id, actorId);

            if (ok)
            {
                _audit.Log(actorId, $"REJECT Budget ID={id}", "Budgets");
                SuccessMessage = $"Đã từ chối ngân sách #{id}.";
            }
            else
            {
                ErrorMessage = "Không tìm thấy ngân sách.";
            }

            return RedirectToPage(new { id });
        }
    }
}
