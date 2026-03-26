using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Project_PRN222.Pages.StaffMKT
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public IndexModel(ProjectPrn222Context db) => _db = db;

        public int RunningCount { get; set; }
        public int PausedCount { get; set; }
        public int CompletedCount { get; set; }
        public decimal TotalAdSpend { get; set; }
        public decimal AvgEngagementRate { get; set; }
        public List<MarketingCampaign> RecentCampaigns { get; set; } = new();

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

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            var eventIds = await _db.Tasks
                .AsNoTracking()
                .Where(t => t.AssignedTo == actorId && t.EventId != null)
                .Select(t => t.EventId!.Value)
                .Distinct()
                .ToListAsync();

            IQueryable<MarketingCampaign> query = _db.MarketingCampaigns
                .AsNoTracking()
                .Include(c => c.Event);

            if (eventIds.Any())
                query = query.Where(c => c.EventId != null && eventIds.Contains(c.EventId.Value));

            var grouped = await query
                .GroupBy(c => c.Status ?? "")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var g in grouped)
            {
                if (string.Equals(g.Status, "Running", StringComparison.OrdinalIgnoreCase))
                    RunningCount = g.Count;
                else if (string.Equals(g.Status, "Paused", StringComparison.OrdinalIgnoreCase))
                    PausedCount = g.Count;
                else if (string.Equals(g.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                    CompletedCount = g.Count;
            }

            TotalAdSpend = await query.SumAsync(c => c.AdSpend ?? 0m);

            AvgEngagementRate = await query.AnyAsync()
                ? await query.AverageAsync(c => c.EngagementRate ?? 0m)
                : 0m;

            RecentCampaigns = await query
                .OrderByDescending(c => c.CampaignId)
                .Take(8)
                .ToListAsync();

            return Page();
        }
    }
}
