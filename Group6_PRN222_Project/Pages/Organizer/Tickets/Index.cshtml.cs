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

        public IList<Event> Events { get; set; } = new List<Event>();
        public Dictionary<int, List<Ticket>> TicketsByEvent { get; set; } = new Dictionary<int, List<Ticket>>();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            var organizerId = HttpContext.Session.GetInt32("UserID");
            var selectedId = HttpContext.Session.GetInt32("SelectedEventId");

            // Get events for this organizer
            var eventsQuery = _context.Events.AsQueryable();
            if (organizerId.HasValue)
                eventsQuery = eventsQuery.Where(e => e.OrganizerId == organizerId);
            if (selectedId.HasValue)
                eventsQuery = eventsQuery.Where(e => e.EventId == selectedId);

            Events = await eventsQuery.OrderByDescending(e => e.EventId).ToListAsync();

            // Load tickets for each event
            var eventIds = Events.Select(e => e.EventId).ToList();
            var allTickets = await _context.Tickets
                .Where(t => t.EventId != null && eventIds.Contains(t.EventId.Value))
                .ToListAsync();

            TicketsByEvent = allTickets
                .GroupBy(t => t.EventId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
