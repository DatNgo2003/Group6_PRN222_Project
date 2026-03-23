using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Group6_PRN222_Project.Pages.Organizer.Tasks
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<Group6_PRN222_Project.Models.Task> Tasks { get; set; } = new List<Group6_PRN222_Project.Models.Task>();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            var selectedId = HttpContext.Session.GetInt32("SelectedEventId");
            var query = _context.Tasks
                .Include(t => t.Event)
                .Include(t => t.AssignedToNavigation)
                .AsQueryable();
            if (selectedId.HasValue)
            {
                query = query.Where(t => t.EventId == selectedId);
            }
            Tasks = await query.ToListAsync();
        }
    }
}
