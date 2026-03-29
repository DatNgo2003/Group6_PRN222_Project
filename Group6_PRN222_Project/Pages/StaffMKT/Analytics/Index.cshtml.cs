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

        // Event-level analytics
        public List<EventAnalyticsRow> Rows { get; set; } = new();

        // Summary statistics
        public int TotalEvents { get; set; }
        public decimal TotalBudgetAllocated { get; set; }
        public decimal TotalSpentAmount { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalParticipants { get; set; }
        public decimal BudgetUtilizationPercent { get; set; }

        // Cost breakdown
        public decimal MarketingSpend { get; set; }
        public decimal BudgetSpend { get; set; }
        public decimal SavingsAmount { get; set; }

        public class EventAnalyticsRow
        {
            public int EventId { get; set; }
            public string EventName { get; set; } = "";
            public string EventStatus { get; set; } = "";
            public DateTime? EventStartDate { get; set; }

            // Marketing metrics
            public int CampaignCount { get; set; }
            public decimal TotalAdSpend { get; set; }
            public decimal AvgEngagement { get; set; }
            public int RunningCount { get; set; }
            public int CompletedCount { get; set; }

            // Budget metrics
            public decimal BudgetAllocated { get; set; }
            public decimal BudgetSpent { get; set; }
            public decimal BudgetRemaining { get; set; }
            public decimal BudgetUtilization { get; set; }
            public decimal Profit { get; set; }

            // Participant metrics
            public int ParticipantCount { get; set; }
            public int TicketsSold { get; set; }
            public decimal TicketRevenue { get; set; }
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

            // Get all events with related data
            var events = await _db.Events
                .AsNoTracking()
                .Include(e => e.MarketingCampaigns)
                .Include(e => e.Budgets)
                .Include(e => e.Tickets)
                .ToListAsync();

            Rows = new List<EventAnalyticsRow>();

            foreach (var evt in events)
            {
                var campaigns = evt.MarketingCampaigns?.ToList() ?? new();
                var budgets = evt.Budgets?.ToList() ?? new();
                var tickets = evt.Tickets?.ToList() ?? new();

                var budgetAllocated = budgets.Sum(b => b.TotalAllocated ?? 0m);
                var budgetSpent = budgets.Sum(b => b.SpentAmount ?? 0m);
                var profit = budgets.Sum(b => b.Profit ?? 0m);
                var adSpend = campaigns.Sum(c => c.AdSpend ?? 0m);
                var ticketsTotal = tickets.Count;
                var ticketsRevenue = tickets.Sum(t => t.Price ?? 0m);
                var avgEngagement = campaigns.Any() ? campaigns.Average(c => c.EngagementRate ?? 0m) : 0m;
                var budgetUtilization = budgetAllocated > 0 ? (budgetSpent / budgetAllocated * 100) : 0m;
                var remaining = budgetAllocated - budgetSpent;

                var row = new EventAnalyticsRow
                {
                    EventId = evt.EventId,
                    EventName = evt.EventName,
                    EventStatus = evt.Status ?? "",
                    EventStartDate = evt.StartDate,

                    CampaignCount = campaigns.Count,
                    TotalAdSpend = adSpend,
                    AvgEngagement = avgEngagement,
                    RunningCount = campaigns.Count(c => c.Status == "Running"),
                    CompletedCount = campaigns.Count(c => c.Status == "Completed"),

                    BudgetAllocated = budgetAllocated,
                    BudgetSpent = budgetSpent,
                    BudgetRemaining = remaining,
                    BudgetUtilization = budgetUtilization,
                    Profit = profit,

                    ParticipantCount = tickets.Select(t => t.ParticipantId).Distinct().Count(),
                    TicketsSold = ticketsTotal,
                    TicketRevenue = ticketsRevenue
                };

                Rows.Add(row);

                // Accumulate summary statistics
                TotalBudgetAllocated += budgetAllocated;
                TotalSpentAmount += budgetSpent;
                TotalProfit += profit;
                MarketingSpend += adSpend;
                BudgetSpend += budgetSpent;
                TotalParticipants += row.ParticipantCount;
            }

            TotalEvents = events.Count;
            BudgetUtilizationPercent = TotalBudgetAllocated > 0
                ? (TotalSpentAmount / TotalBudgetAllocated * 100)
                : 0m;

            SavingsAmount = TotalBudgetAllocated - TotalSpentAmount;

            // Sort by total spent descending, then by campaign count
            Rows = Rows
                .OrderByDescending(r => r.BudgetSpent)
                .ThenByDescending(r => r.CampaignCount)
                .ToList();

            return Page();
        }
    }
}