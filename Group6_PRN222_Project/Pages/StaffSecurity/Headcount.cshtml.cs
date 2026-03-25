using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Helpers;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Pages.StaffSecurity;

[AuthorizeRole("Staff(Security)")]
public class HeadcountModel : PageModel
{
    private readonly ProjectPrn222Context _db;

    public HeadcountModel(ProjectPrn222Context db) => _db = db;

    public List<Event> Events { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int EventId { get; set; }

    public int EligibleTickets { get; set; }
    public int CheckedIn { get; set; }
    public int PendingPayment { get; set; }

    public double CheckInPercent =>
        EligibleTickets > 0 ? Math.Round(100.0 * CheckedIn / EligibleTickets, 1) : 0;

    public async System.Threading.Tasks.Task OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("userid") ?? 0;
        Events = await StaffSecurityEvents.LoadEventsForStaffAsync(_db, userId);

        if (Events.Count == 0)
            return;

        if (EventId <= 0 || !Events.Any(e => e.EventId == EventId))
            EventId = Events[0].EventId;

        EligibleTickets = await _db.Tickets.AsNoTracking()
            .CountAsync(t => t.EventId == EventId
                && t.PaymentStatus == "Paid"
                && (t.Status == null || t.Status == "Valid"));

        CheckedIn = await _db.Tickets.AsNoTracking()
            .CountAsync(t => t.EventId == EventId
                && t.CheckInTime != null
                && t.PaymentStatus == "Paid");

        PendingPayment = await _db.Tickets.AsNoTracking()
            .CountAsync(t => t.EventId == EventId && t.PaymentStatus == "Pending");
    }
}
