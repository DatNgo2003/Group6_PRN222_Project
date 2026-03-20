using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Organizer.Budgets
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<Budget> Budgets { get; set; } = new List<Budget>();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            Budgets = await _context.Budgets
                .Include(b => b.Event)
                .Include(b => b.ApprovedByNavigation)
                .ToListAsync();
        }
    }
}
