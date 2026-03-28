using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

using AppTask = System.Threading.Tasks.Task;

namespace Project_PRN222.Pages.StaffLogistics.Reports
{
    public class CreateModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        private readonly IAuditLogService _audit;

        public CreateModel(ProjectPrn222Context db, IAuditLogService audit)
        {
            _db = db;
            _audit = audit;
        }

        public List<Event> Events { get; set; } = new();

        [BindProperty] public int EventId { get; set; }
        [BindProperty] public string ReportType { get; set; } = "Logistics";
        [BindProperty] public string ReportContent { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(string? reportType)
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

            ReportType = string.IsNullOrWhiteSpace(reportType) ? "Logistics" : reportType.Trim();
            if (!string.Equals(ReportType, "Logistics", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(ReportType, "Equipment", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(ReportType, "Issue", StringComparison.OrdinalIgnoreCase))
                ReportType = "Logistics";

            await LoadEventsForActorAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
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

            ReportContent = ReportContent?.Trim() ?? "";
            if (EventId <= 0)
                ModelState.AddModelError(nameof(EventId), "Vui lòng chọn sự kiện hợp lệ.");
            if (string.IsNullOrWhiteSpace(ReportType))
                ModelState.AddModelError(nameof(ReportType), "Vui lòng chọn loại báo cáo.");
            if (string.IsNullOrWhiteSpace(ReportContent))
                ModelState.AddModelError(nameof(ReportContent), "Nội dung báo cáo không được để trống.");

            if (!ModelState.IsValid)
            {
                await LoadEventsForActorAsync();
                return Page();
            }

            // Normalize ReportType for DB consistency
            if (string.Equals(ReportType, "Equipment", StringComparison.OrdinalIgnoreCase))
                ReportType = "Equipment";
            else if (string.Equals(ReportType, "Issue", StringComparison.OrdinalIgnoreCase))
                ReportType = "Issue";
            else
                ReportType = "Logistics";

            var report = new FieldReport
            {
                EventId = EventId,
                StaffId = actorId,
                ReportType = ReportType,
                Content = ReportContent,
                Status = "Submitted"
            };

            _db.FieldReports.Add(report);
            await _db.SaveChangesAsync();

            _audit.Log(actorId, $"CREATE FieldReport ReportType='{ReportType}' EventId={EventId}", "FieldReports");

            TempData["Success"] = "Tạo báo cáo thành công.";
            return RedirectToPage("Index");
        }

        private async AppTask LoadEventsForActorAsync()
        {
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;

            // Ưu tiên lấy các event có task được gán cho staff
            var eventIds = await _db.Tasks
                .AsNoTracking()
                .Where(t => t.AssignedTo == actorId && t.EventId != null)
                .Select(t => t.EventId!.Value)
                .Distinct()
                .ToListAsync();

            IQueryable<Event> q = _db.Events.AsNoTracking();
            if (eventIds.Any())
                q = q.Where(e => eventIds.Contains(e.EventId));

            Events = await q
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            if (Events.Any() && EventId <= 0)
                EventId = Events.First().EventId;
        }
    }
}

