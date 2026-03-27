using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Pages.Events
{
    public class EventListModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        public EventListModel(ProjectPrn222Context db) => _db = db;

        public List<Event> Events { get; set; } = new();
        public int TotalCount { get; set; }

        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public string? Status { get; set; }
        [BindProperty(SupportsGet = true)] public string? Location { get; set; }
        [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;

        public const int PageSize = 9;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            var query = _db.Events
                .Include(e => e.Organizer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
                query = query.Where(e =>
                    e.EventName.Contains(Search) ||
                    (e.Location != null && e.Location.Contains(Search)));

            if (!string.IsNullOrWhiteSpace(Status))
                query = query.Where(e => e.Status == Status);

            if (!string.IsNullOrWhiteSpace(Location))
                query = query.Where(e => e.Location != null && e.Location.Contains(Location));

            TotalCount = await query.CountAsync();

            Events = await query
                .OrderByDescending(e => e.StartDate)
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
