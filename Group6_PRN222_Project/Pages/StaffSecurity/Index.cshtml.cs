using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Helpers;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Pages.StaffSecurity;

[AuthorizeRole("Staff(Security)")]
public class IndexModel : PageModel
{
    private readonly ProjectPrn222Context _db;

    public IndexModel(ProjectPrn222Context db) => _db = db;

    public string FullName { get; set; } = "";
    public int VipCount { get; set; }
    public int BlacklistCount { get; set; }
    public int CheckedInToday { get; set; }
    public int SecurityReportsCount { get; set; }
    public int OpenTasks { get; set; }

    public async System.Threading.Tasks.Task OnGetAsync()
    {
        FullName = HttpContext.Session.GetString("fullname") ?? "An ninh";
        var userId = HttpContext.Session.GetInt32("userid") ?? 0;

        var events = await StaffSecurityEvents.LoadEventsForStaffAsync(_db, userId);
        var eventIds = events.Select(e => e.EventId).ToList();

        if (eventIds.Count > 0)
        {
            VipCount = await _db.Tickets.AsNoTracking()
                .Where(t => t.EventId != null && eventIds.Contains(t.EventId.Value))
                .Where(t => t.Participant != null && t.Participant.IsVip == true)
                .Select(t => t.ParticipantId!.Value)
                .Distinct()
                .CountAsync();

            BlacklistCount = await _db.Tickets.AsNoTracking()
                .Where(t => t.EventId != null && eventIds.Contains(t.EventId.Value))
                .Where(t => t.Participant != null && t.Participant.IsBlacklisted == true)
                .Select(t => t.ParticipantId!.Value)
                .Distinct()
                .CountAsync();

            var today = DateTime.Today;
            CheckedInToday = await _db.Tickets.AsNoTracking()
                .Where(t => t.EventId != null && eventIds.Contains(t.EventId.Value)
                    && t.CheckInTime != null && t.CheckInTime.Value.Date == today)
                .CountAsync();
        }

        SecurityReportsCount = await _db.FieldReports.AsNoTracking()
            .CountAsync(r => r.StaffId == userId && r.ReportType == "Security");

        OpenTasks = await _db.Tasks.AsNoTracking()
            .CountAsync(t => t.AssignedTo == userId && t.Status != "Done");
    }
}
