using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Group6_PRN222_Project.Pages.Organizer
{
    public class DashboardModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public DashboardModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public int TotalEvents { get; set; }
        public int OngoingEvents { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public decimal TotalBudgetAllocated { get; set; }
        public decimal TotalBudgetSpent { get; set; }



        public List<string> BudgetEventNames { get; set; } = new();
        public List<decimal> BudgetAllocatedData { get; set; } = new();
        public List<decimal> BudgetSpentData { get; set; } = new();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            var selectedId = HttpContext.Session.GetInt32("SelectedEventId");
            
            var eventQuery = _context.Events.AsQueryable();
            var taskQuery = _context.Tasks.AsQueryable();
            var budgetQuery = _context.Budgets.AsQueryable();

            if (selectedId.HasValue)
            {
                eventQuery = eventQuery.Where(e => e.EventId == selectedId);
                taskQuery = taskQuery.Where(t => t.EventId == selectedId);
                budgetQuery = budgetQuery.Where(b => b.EventId == selectedId);
                TotalEvents = 1;
            }
            else
            {
                TotalEvents = await eventQuery.CountAsync();
            }

            OngoingEvents = await eventQuery.CountAsync(e => e.Status == "Ongoing" || e.Status == "Planning" || e.Status == "Active");
            
            TotalTasks = await taskQuery.CountAsync();
            CompletedTasks = await taskQuery.CountAsync(t => t.Status == "Completed" || t.Status == "Done");
            
            TotalBudgetAllocated = await budgetQuery.SumAsync(b => b.TotalAllocated) ?? 0m;
            TotalBudgetSpent = await budgetQuery.SumAsync(b => b.SpentAmount) ?? 0m;



            var topBudgets = await budgetQuery
                .Include(b => b.Event)
                .Where(b => b.Event != null)
                .OrderByDescending(b => b.TotalAllocated)
                .Take(5)
                .ToListAsync();

            BudgetEventNames = topBudgets.Select(b => b.Event!.EventName).ToList();
            BudgetAllocatedData = topBudgets.Select(b => b.TotalAllocated ?? 0).ToList();
            BudgetSpentData = topBudgets.Select(b => b.SpentAmount ?? 0).ToList();
        }
    }
}
