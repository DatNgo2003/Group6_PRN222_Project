using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Helpers;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Services;

namespace Group6_PRN222_Project.Pages.StaffSecurity.Reports;

[AuthorizeRole("Staff(Security)")]
public class CreateModel : PageModel
{
    private readonly ProjectPrn222Context _db;
    private readonly IAuditLogService _audit;

    public CreateModel(ProjectPrn222Context db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public List<Event> Events { get; set; } = new();

    [BindProperty]
    public int EventId { get; set; }

    [BindProperty]
    public string ReportContent { get; set; } = "";

    public async System.Threading.Tasks.Task OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("userid") ?? 0;
        Events = await StaffSecurityEvents.LoadEventsForStaffAsync(_db, userId);
        if (Events.Count > 0 && (EventId <= 0 || !Events.Any(e => e.EventId == EventId)))
            EventId = Events[0].EventId;
    }

    public async System.Threading.Tasks.Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("userid") ?? 0;
        Events = await StaffSecurityEvents.LoadEventsForStaffAsync(_db, userId);

        ReportContent = ReportContent?.Trim() ?? "";
        if (EventId <= 0 || !Events.Any(e => e.EventId == EventId))
            ModelState.AddModelError(nameof(EventId), "Vui lòng chọn sự kiện hợp lệ.");
        if (string.IsNullOrWhiteSpace(ReportContent))
            ModelState.AddModelError(nameof(ReportContent), "Nội dung báo cáo không được để trống.");

        if (!ModelState.IsValid)
            return Page();

        var report = new FieldReport
        {
            EventId = EventId,
            StaffId = userId,
            ReportType = "Security",
            Content = ReportContent,
            ReportTime = DateTime.Now,
            Status = "Submitted"
        };

        _db.FieldReports.Add(report);
        await _db.SaveChangesAsync();

        _audit.Log(userId, "CREATE FieldReport Security EventId=" + EventId, "FieldReports");

        TempData["Success"] = "Đã gửi báo cáo an ninh.";
        return RedirectToPage("Index");
    }
}
