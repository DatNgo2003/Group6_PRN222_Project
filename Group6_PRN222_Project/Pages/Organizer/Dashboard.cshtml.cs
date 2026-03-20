using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            TotalEvents = await _context.Events.CountAsync();
            OngoingEvents = await _context.Events.CountAsync(e => e.Status == "Ongoing" || e.Status == "Planning");
            
            TotalTasks = await _context.Tasks.CountAsync();
            CompletedTasks = await _context.Tasks.CountAsync(t => t.Status == "Completed" || t.Status == "Done");
            
            TotalBudgetAllocated = await _context.Budgets.SumAsync(b => b.TotalAllocated) ?? 0m;
            TotalBudgetSpent = await _context.Budgets.SumAsync(b => b.SpentAmount) ?? 0m;
        }
    }
}
