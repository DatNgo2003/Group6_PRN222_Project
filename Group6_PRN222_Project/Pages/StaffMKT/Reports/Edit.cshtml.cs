using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Project_PRN222.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Services;
using Task = System.Threading.Tasks.Task;

namespace Project_PRN222.Pages.StaffMKT.Reports
{
    public class EditModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        private readonly IAuditLogService _audit;

        public EditModel(ProjectPrn222Context db, IAuditLogService audit)
        {
            _db = db;
            _audit = audit;
        }

        public int ReportId { get; set; }

        public List<Event> Events { get; set; } = new();

        public List<TaskSnapshot> TaskSnapshots { get; set; } = new();

        public class TaskSnapshot
        {
            public int TaskId { get; set; }
            public string TaskName { get; set; } = "";
            public DateTime? Deadline { get; set; }
        }

        public class TaskLineInput
        {
            public int TaskId { get; set; }
            public decimal? Amount { get; set; }
            public string? Note { get; set; }
        }

        [BindProperty] public int EventId { get; set; }
        [BindProperty] public string Changes { get; set; } = "";
        [BindProperty] public string ReportBody { get; set; } = "";
        [BindProperty] public List<TaskLineInput> Lines { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id, int? eventId)
        {
            if (!EnsureMkt()) return RedirectToPage("/Auth/Login");

            ReportId = id;
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;

            var report = await _db.FieldReports.AsNoTracking().FirstOrDefaultAsync(r =>
                r.ReportId == id && r.StaffId == actorId && r.ReportType == "Marketing");

            if (report == null)
                return NotFound();

            if (report.Status != "Draft")
            {
                TempData["Error"] = "Chỉ có thể sửa báo cáo đang ở trạng thái nháp.";
                return RedirectToPage("Index");
            }

            await LoadEventsAsync();

            EventId = eventId ?? report.EventId ?? 0;
            if (Events.Any() && !Events.Any(e => e.EventId == EventId))
                EventId = Events.First().EventId;

            var payload = MarketingFieldReportContent.Parse(report.Content);
            Changes = payload.Changes ?? "";
            ReportBody = payload.ReportBody ?? "";

            if (eventId.HasValue)
                await RefreshTaskSnapshotsAndLinesAsync(actorId, mergeFromPost: false);
            else
                await RefreshTaskSnapshotsAndLinesAsync(actorId, mergeFromPayload: payload);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, string action)
        {
            if (!EnsureMkt()) return RedirectToPage("/Auth/Login");

            ReportId = id;
            action = action?.Trim().ToLowerInvariant() ?? "";
            var isDraft = action == "draft";

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;

            var report = await _db.FieldReports.FirstOrDefaultAsync(r =>
                r.ReportId == id && r.StaffId == actorId && r.ReportType == "Marketing");

            if (report == null)
                return NotFound();

            if (report.Status != "Draft")
            {
                TempData["Error"] = "Chỉ có thể sửa báo cáo đang ở trạng thái nháp.";
                return RedirectToPage("Index");
            }

            await LoadEventsAsync();

            var allowedEventIds = await _db.Tasks.AsNoTracking()
                .Where(t => t.AssignedTo == actorId && t.EventId != null)
                .Select(t => t.EventId!.Value)
                .Distinct()
                .ToListAsync();

            if (!allowedEventIds.Contains(EventId))
                ModelState.AddModelError(nameof(EventId), "Sự kiện không hợp lệ.");

            await RefreshTaskSnapshotsAndLinesAsync(actorId, mergeFromPost: true);

            Changes = Changes?.Trim() ?? "";
            ReportBody = ReportBody?.Trim() ?? "";

            if (!isDraft && string.IsNullOrWhiteSpace(ReportBody))
                ModelState.AddModelError(nameof(ReportBody), "Nội dung báo cáo không được để trống khi gửi.");

            if (!ModelState.IsValid)
                return Page();

            var payload = new MarketingFieldReportPayload
            {
                Changes = string.IsNullOrWhiteSpace(Changes) ? null : Changes,
                ReportBody = string.IsNullOrWhiteSpace(ReportBody) ? null : ReportBody,
                TaskEstimates = Lines.Select(l => new MarketingTaskEstimateRow
                {
                    TaskId = l.TaskId,
                    Amount = l.Amount,
                    Note = string.IsNullOrWhiteSpace(l.Note) ? null : l.Note.Trim()
                }).ToList()
            };

            var total = MarketingFieldReportContent.SumEstimates(payload);

            report.EventId = EventId;
            report.Content = MarketingFieldReportContent.Serialize(payload);
            report.EstimatePrice = total;
            report.Status = isDraft ? "Draft" : "Submitted";
            report.ReportTime = DateTime.Now;

            await _db.SaveChangesAsync();

            _audit.Log(actorId, $"UPDATE FieldReport Marketing ReportId={id} Draft={isDraft}", "FieldReports");

            TempData["Success"] = isDraft ? "Đã cập nhật nháp." : "Đã gửi báo cáo cho Organizer.";
            return RedirectToPage("Index");
        }

        private bool EnsureMkt()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", RoleConstants.StaffMKT);
                HttpContext.Session.SetString("FullName", "Dev Marketing");
                HttpContext.Session.SetString("UserName", "marketing");
            }
            return true;
#else
            var isLogged = SessionHelper.IsLoggedIn(HttpContext.Session);
            var role = SessionHelper.GetRole(HttpContext.Session);
            return isLogged && role == RoleConstants.StaffMKT;
#endif
        }

        private async Task LoadEventsAsync()
        {
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;
            var eventIds = await _db.Tasks.AsNoTracking()
                .Where(t => t.AssignedTo == actorId && t.EventId != null)
                .Select(t => t.EventId!.Value)
                .Distinct()
                .ToListAsync();

            Events = await _db.Events.AsNoTracking()
                .Where(e => eventIds.Contains(e.EventId))
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        private async Task RefreshTaskSnapshotsAndLinesAsync(int actorId, MarketingFieldReportPayload? mergeFromPayload = null, bool mergeFromPost = false)
        {
            TaskSnapshots = await _db.Tasks.AsNoTracking()
                .Where(t => t.AssignedTo == actorId && t.EventId == EventId)
                .OrderBy(t => t.Deadline)
                .Select(t => new TaskSnapshot
                {
                    TaskId = t.TaskId,
                    TaskName = t.TaskName ?? "",
                    Deadline = t.Deadline
                })
                .ToListAsync();

            if (mergeFromPayload != null)
            {
                var byTask = mergeFromPayload.TaskEstimates.ToDictionary(x => x.TaskId, x => x);
                Lines = TaskSnapshots.Select(s =>
                {
                    byTask.TryGetValue(s.TaskId, out var row);
                    return new TaskLineInput
                    {
                        TaskId = s.TaskId,
                        Amount = row?.Amount,
                        Note = row?.Note
                    };
                }).ToList();
                return;
            }

            if (!mergeFromPost || Lines == null || !Lines.Any())
            {
                Lines = TaskSnapshots.Select(s => new TaskLineInput
                {
                    TaskId = s.TaskId,
                    Amount = null,
                    Note = null
                }).ToList();
                return;
            }

            var posted = Lines.ToDictionary(x => x.TaskId, x => x);
            Lines = TaskSnapshots.Select(s =>
            {
                posted.TryGetValue(s.TaskId, out var pl);
                return new TaskLineInput
                {
                    TaskId = s.TaskId,
                    Amount = pl?.Amount,
                    Note = pl?.Note
                };
            }).ToList();
        }
    }
}
