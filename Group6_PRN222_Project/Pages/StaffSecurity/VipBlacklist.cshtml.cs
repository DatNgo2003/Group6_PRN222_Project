using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Helpers;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Pages.StaffSecurity;

[AuthorizeRole("Staff(Security)")]
public class VipBlacklistModel : PageModel
{
    private readonly ProjectPrn222Context _db;

    public VipBlacklistModel(ProjectPrn222Context db) => _db = db;

    public List<Event> Events { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int EventId { get; set; }

    public List<VipBlacklistRow> Rows { get; set; } = new();

    public class VipBlacklistRow
    {
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool IsVip { get; set; }
        public bool IsBlacklisted { get; set; }
        public string PaymentStatus { get; set; } = "";
        public Guid? Qrcode { get; set; }
    }

    public async System.Threading.Tasks.Task OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("userid") ?? 0;
        Events = await StaffSecurityEvents.LoadEventsForStaffAsync(_db, userId);

        if (Events.Count == 0)
            return;

        if (EventId <= 0 || !Events.Any(e => e.EventId == EventId))
            EventId = Events[0].EventId;

        Rows = await _db.Tickets.AsNoTracking()
            .Where(t => t.EventId == EventId && t.Participant != null)
            .Where(t => t.Participant!.IsVip == true || t.Participant!.IsBlacklisted == true)
            .OrderByDescending(t => t.Participant!.IsBlacklisted)
            .ThenByDescending(t => t.Participant!.IsVip)
            .ThenBy(t => t.Participant!.FullName)
            .Select(t => new VipBlacklistRow
            {
                FullName = t.Participant!.FullName ?? "—",
                Email = t.Participant.Email,
                Phone = t.Participant.Phone,
                IsVip = t.Participant.IsVip == true,
                IsBlacklisted = t.Participant.IsBlacklisted == true,
                PaymentStatus = t.PaymentStatus ?? "—",
                Qrcode = t.Qrcode
            })
            .ToListAsync();
    }
}
