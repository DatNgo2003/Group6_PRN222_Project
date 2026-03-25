using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Group6_PRN222_Project.Pages.Organizer.Tickets
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<Ticket> Tickets { get; set; } = new List<Ticket>();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            var selectedId = HttpContext.Session.GetInt32("SelectedEventId");
            var query = _context.Tickets
                .Include(t => t.Event)
                .Include(t => t.Participant)
                .AsQueryable();

            if (selectedId.HasValue)
            {
                query = query.Where(t => t.EventId == selectedId);
            }

            Tickets = await query.OrderByDescending(t => t.TicketId).ToListAsync();
        }
    }
}
