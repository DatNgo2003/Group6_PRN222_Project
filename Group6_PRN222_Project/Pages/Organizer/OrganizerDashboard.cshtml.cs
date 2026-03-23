using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Pages.Organizer
{
    [AuthorizeRole("Organizer")]
    public class OrganizerDashboardModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public OrganizerDashboardModel(ProjectPrn222Context db)
        {
            _db = db;
        }

        public string FullName { get; set; } = "";
        public int MyTotalEvents { get; set; }
        public int MyActiveEvents { get; set; }
        public int MyTotalTasks { get; set; }
        public int MyPendingTasks { get; set; }
        public List<Event> MyEvents { get; set; } = new();
        public List<Group6_PRN222_Project.Models.Task> MyTasks { get; set; } = new();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            FullName = HttpContext.Session.GetString("fullname") ?? "Organizer";
            var userId = HttpContext.Session.GetInt32("userid") ?? 0;

            MyTotalEvents = await _db.Events.CountAsync(e => e.OrganizerId == userId);
            MyActiveEvents = await _db.Events.CountAsync(e => e.OrganizerId == userId && e.Status == "Active");
            MyTotalTasks = await _db.Tasks.CountAsync(t => t.Event != null && t.Event.OrganizerId == userId);
            MyPendingTasks = await _db.Tasks.CountAsync(t => t.Event != null && t.Event.OrganizerId == userId && t.Status == "To Do");

            MyEvents = await _db.Events
                .Where(e => e.OrganizerId == userId)
                .OrderByDescending(e => e.StartDate)
                .Take(6)
                .ToListAsync();

            MyTasks = await _db.Tasks
                .Include(t => t.Event)
                .Include(t => t.AssignedToNavigation)
                .Where(t => t.Event != null && t.Event.OrganizerId == userId)
                .OrderBy(t => t.Deadline)
                .Take(6)
                .ToListAsync();
        }
    }
}
