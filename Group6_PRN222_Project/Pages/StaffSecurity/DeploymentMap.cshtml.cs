using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Helpers;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AppTask = Group6_PRN222_Project.Models.Task;

namespace Group6_PRN222_Project.Pages.StaffSecurity;

[AuthorizeRole("Staff(Security)")]
public class DeploymentMapModel : PageModel
{
    private readonly ProjectPrn222Context _db;

    public DeploymentMapModel(ProjectPrn222Context db) => _db = db;

    public List<Event> Events { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int EventId { get; set; }

    public Event? CurrentEvent { get; set; }
    public List<AppTask> MySecurityTasks { get; set; } = new();

    public async System.Threading.Tasks.Task OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("userid") ?? 0;
        Events = await StaffSecurityEvents.LoadEventsForStaffAsync(_db, userId);

        if (Events.Count == 0)
            return;

        if (EventId <= 0 || !Events.Any(e => e.EventId == EventId))
            EventId = Events[0].EventId;

        CurrentEvent = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == EventId);

        MySecurityTasks = await _db.Tasks.AsNoTracking()
            .Where(t => t.EventId == EventId && t.AssignedTo == userId)
            .OrderBy(t => t.Deadline)
            .ToListAsync();
    }
}
