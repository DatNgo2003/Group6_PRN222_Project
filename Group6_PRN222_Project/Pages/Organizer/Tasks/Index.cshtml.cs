using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Organizer.Tasks
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<EventTask> Tasks { get; set; } = new List<EventTask>();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            Tasks = await _context.Tasks
                .Include(t => t.Event)
                .Include(t => t.AssignedToNavigation)
                .ToListAsync();
        }
    }
}
