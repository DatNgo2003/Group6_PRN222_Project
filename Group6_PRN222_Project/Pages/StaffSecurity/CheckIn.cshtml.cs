using System.Text.RegularExpressions;
using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Helpers;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Services;

namespace Group6_PRN222_Project.Pages.StaffSecurity;

[AuthorizeRole("Staff(Security)")]
public class CheckInModel : PageModel
{
    private static readonly Regex GuidRx = new Regex(
        "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
        RegexOptions.Compiled);

    private readonly ProjectPrn222Context _db;
    private readonly IAuditLogService _audit;

    public CheckInModel(ProjectPrn222Context db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public List<Event> Events { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int EventId { get; set; }

    [BindProperty]
    public string QrInput { get; set; } = "";

    public string? ResultMessage { get; set; }
    public bool ResultSuccess { get; set; }

    public async System.Threading.Tasks.Task OnGetAsync()
    {
        await LoadEventsAsync();
    }

    public async System.Threading.Tasks.Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("userid") ?? 0;
        await LoadEventsAsync();

        if (EventId > 0 && !Events.Any(e => e.EventId == EventId))
            ModelState.AddModelError(nameof(EventId), "Sự kiện không hợp lệ.");

        var raw = QrInput?.Trim() ?? "";
        if (string.IsNullOrEmpty(raw))
            ModelState.AddModelError(nameof(QrInput), "Vui lòng nhập hoặc quét mã QR (GUID trên vé).");

        if (!TryParseGuidFromInput(raw, out var guid))
            ModelState.AddModelError(nameof(QrInput), "Không đọc được mã vé. Dán đúng chuỗi GUID hoặc URL chứa GUID.");

        if (!ModelState.IsValid)
            return Page();

        var q = _db.Tickets
            .Include(t => t.Participant)
            .Include(t => t.Event)
            .Where(t => t.Qrcode == guid);

        if (EventId > 0)
            q = q.Where(t => t.EventId == EventId);

        var ticket = await q.FirstOrDefaultAsync();

        if (ticket == null)
        {
            ResultMessage = "Không tìm thấy vé với mã này (hoặc không thuộc sự kiện đã chọn).";
            ResultSuccess = false;
            return Page();
        }

        if (ticket.Participant?.IsBlacklisted == true)
        {
            ResultMessage = "Khách nằm trong blacklist — không cho vào.";
            ResultSuccess = false;
            return Page();
        }

        if (ticket.PaymentStatus != "Paid")
        {
            ResultMessage = "Vé chưa thanh toán (trạng thái: " + (ticket.PaymentStatus ?? "—") + ").";
            ResultSuccess = false;
            return Page();
        }

        if (ticket.Status is { } s && s != "Valid")
        {
            ResultMessage = "Vé không hợp lệ (Status: " + ticket.Status + ").";
            ResultSuccess = false;
            return Page();
        }

        if (ticket.CheckInTime != null)
        {
            ResultMessage = "Đã check-in trước đó lúc " + ticket.CheckInTime.Value.ToString("dd/MM/yyyy HH:mm") + " — " + (ticket.Participant?.FullName ?? "Khách") + ".";
            ResultSuccess = true;
            return Page();
        }

        ticket.CheckInTime = DateTime.Now;
        await _db.SaveChangesAsync();

        _audit.Log(userId, "CHECKIN TicketId=" + ticket.TicketId + " EventId=" + ticket.EventId + " QR=" + guid, "Tickets");

        ResultMessage = "Check-in thành công: " + (ticket.Participant?.FullName ?? "Khách") + " — " + (ticket.Event?.EventName ?? "Sự kiện") + ".";
        ResultSuccess = true;
        QrInput = "";
        return Page();
    }

    private async System.Threading.Tasks.Task LoadEventsAsync()
    {
        var userId = HttpContext.Session.GetInt32("userid") ?? 0;
        Events = await StaffSecurityEvents.LoadEventsForStaffAsync(_db, userId);
        if (Events.Count > 0 && (EventId <= 0 || !Events.Any(e => e.EventId == EventId)))
            EventId = Events[0].EventId;
    }

    private static bool TryParseGuidFromInput(string raw, out Guid guid)
    {
        guid = default;
        var compact = raw.Replace("{", "").Replace("}", "").Trim();
        if (Guid.TryParse(compact, out guid))
            return true;

        var m = GuidRx.Match(raw);
        if (m.Success && Guid.TryParse(m.Value, out guid))
            return true;

        return false;
    }
}
