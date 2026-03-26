using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Group6_PRN222_Project.Models;

namespace Project_PRN222.Pages.StaffMKT.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        public IndexModel(ProjectPrn222Context db) => _db = db;

        public List<EngagementReportRow> Rows { get; set; } = new();

        public class EngagementReportRow
        {
            public string EventName { get; set; } = "";
            public int CampaignCount { get; set; }
            public decimal TotalAdSpend { get; set; }
            public decimal AvgEngagement { get; set; }
            public string OverallStatus { get; set; } = "Running";
        }

        public async Task<IActionResult> OnGetAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", RoleConstants.StaffMKT);
                HttpContext.Session.SetString("FullName", "Dev Marketing");
                HttpContext.Session.SetString("UserName", "marketing");
            }
#else
            var isLogged = SessionHelper.IsLoggedIn(HttpContext.Session);
            var role = SessionHelper.GetRole(HttpContext.Session);
            if (!isLogged || role != RoleConstants.StaffMKT)
                return RedirectToPage("/Auth/Login");
#endif

            Rows = await _db.MarketingCampaigns
                .AsNoTracking()
                .Include(c => c.Event)
                .GroupBy(c => c.Event != null ? c.Event.EventName : "N/A")
                .Select(g => new EngagementReportRow
                {
                    EventName = g.Key,
                    CampaignCount = g.Count(),
                    TotalAdSpend = g.Sum(x => x.AdSpend ?? 0m),
                    AvgEngagement = g.Average(x => x.EngagementRate ?? 0m),
                    OverallStatus = g.Any(x => x.Status == "Running")
                        ? "Running"
                        : g.All(x => x.Status == "Completed") ? "Completed" : "Paused"
                })
                .OrderByDescending(x => x.TotalAdSpend)
                .ToListAsync();

            return Page();
        }
    }
}
