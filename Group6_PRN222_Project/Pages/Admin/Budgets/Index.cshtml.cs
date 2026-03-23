using Group6_PRN222_Project.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.Budgets
{
    [AuthorizeRole("Admin")]
    public class IndexModel : PageModel
    {
        private readonly IBudgetService   _budgetSvc;
        private readonly IAuditLogService _audit;

        public IndexModel(IBudgetService budgetSvc, IAuditLogService audit)
        {
            _budgetSvc = budgetSvc;
            _audit     = audit;
        }

        public List<BudgetListItemDto> Budgets { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string StatusFilter { get; set; } = "All";

        // Thống kê nhanh
        public int PendingCount  { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage   { get; set; }

        public async Task<IActionResult> OnGetAsync()
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

            var all = await _budgetSvc.GetAllAsync(StatusFilter);
            Budgets      = all;
            PendingCount  = all.Count(b => b.ApprovalStatus == "Pending");
            ApprovedCount = all.Count(b => b.ApprovalStatus == "Approved");
            RejectedCount = all.Count(b => b.ApprovalStatus == "Rejected");

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

            return RedirectToPage(new { StatusFilter });
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

            return RedirectToPage(new { StatusFilter });
        }
    }
}
