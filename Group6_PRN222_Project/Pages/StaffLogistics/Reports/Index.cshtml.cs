using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Project_PRN222.Pages.StaffLogistics.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public IndexModel(ProjectPrn222Context db) => _db = db;

        public List<FieldReport> Reports { get; set; } = new();
        public List<Event> Events { get; set; } = new();

        [BindProperty(SupportsGet = true)] public int? FilterEventId { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterReportType { get; set; }

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

            Events = await _db.Events
                .AsNoTracking()
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            var query = _db.FieldReports
                .AsNoTracking()
                .Include(r => r.Event)
                .Where(r => r.StaffId == actorId);

            if (FilterEventId.HasValue)
                query = query.Where(r => r.EventId == FilterEventId);

            if (!string.IsNullOrWhiteSpace(FilterReportType) && !string.Equals(FilterReportType, "All", StringComparison.OrdinalIgnoreCase))
                query = query.Where(r => r.ReportType == FilterReportType);

            Reports = await query
                .OrderByDescending(r => r.ReportTime)
                .ThenByDescending(r => r.ReportId)
                .ToListAsync();

            return Page();
        }
    }
}

