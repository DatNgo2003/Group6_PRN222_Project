using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Project_PRN222.Pages.StaffMKT.Analytics
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public IndexModel(ProjectPrn222Context db) => _db = db;

        public List<EventAnalyticsRow> Rows { get; set; } = new();

        public class EventAnalyticsRow
        {
            public string EventName { get; set; } = "";
            public int CampaignCount { get; set; }
            public decimal TotalAdSpend { get; set; }
            public decimal AvgEngagement { get; set; }
            public int RunningCount { get; set; }
            public int CompletedCount { get; set; }
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
                .Select(g => new EventAnalyticsRow
                {
                    EventName = g.Key,
                    CampaignCount = g.Count(),
                    TotalAdSpend = g.Sum(x => x.AdSpend ?? 0m),
                    AvgEngagement = g.Average(x => x.EngagementRate ?? 0m),
                    RunningCount = g.Count(x => x.Status == "Running"),
                    CompletedCount = g.Count(x => x.Status == "Completed")
                })
                .OrderByDescending(r => r.TotalAdSpend)
                .ThenByDescending(r => r.CampaignCount)
                .ToListAsync();

            return Page();
        }
    }
}
