using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Organizer.Events
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<Event> Events { get; set; } = new List<Event>();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            Events = await _context.Events.Include(e => e.Organizer).ToListAsync();
        }
    }
}
