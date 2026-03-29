using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Project_PRN222.Services;
using Task = System.Threading.Tasks.Task;

namespace Project_PRN222.Pages.StaffMKT.Reports
{
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

        public async Task<IActionResult> OnGetAsync(int? eventId)
        {
            if (!EnsureMkt()) return RedirectToPage("/Auth/Login");

            await LoadEventsAsync();
            if (!Events.Any())
                return Page();

            EventId = eventId ?? Events.First().EventId;
            if (!Events.Any(e => e.EventId == EventId))
                EventId = Events.First().EventId;

            await RefreshTaskSnapshotsAndLinesAsync(mergeFromPost: false);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string action)
        {
            if (!EnsureMkt()) return RedirectToPage("/Auth/Login");

            action = action?.Trim().ToLowerInvariant() ?? "";
            var isDraft = action == "draft";

            await LoadEventsAsync();
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;

            var allowedEventIds = await _db.Tasks.AsNoTracking()
                .Where(t => t.AssignedTo == actorId && t.EventId != null)
                .Select(t => t.EventId!.Value)
                .Distinct()
                .ToListAsync();

            if (!allowedEventIds.Contains(EventId))
                ModelState.AddModelError(nameof(EventId), "Sự kiện không hợp lệ.");

            await RefreshTaskSnapshotsAndLinesAsync(mergeFromPost: true);

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

            var report = new FieldReport
            {
                EventId = EventId,
                StaffId = actorId,
                ReportType = "Marketing",
                Content = MarketingFieldReportContent.Serialize(payload),
                EstimatePrice = total,
                Status = isDraft ? "Draft" : "Submitted",
                ReportTime = DateTime.Now
            };

            _db.FieldReports.Add(report);
            await _db.SaveChangesAsync();

            _audit.Log(actorId, $"CREATE FieldReport Marketing EventId={EventId} Draft={isDraft}", "FieldReports");

            TempData["Success"] = isDraft ? "Đã lưu nháp báo cáo." : "Đã gửi báo cáo cho Organizer.";
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

        /// <summary>
        /// mergeFromPost: sau POST, ghép Lines đã bind với danh sách task hợp lệ theo EventId.
        /// </summary>
        private async Task RefreshTaskSnapshotsAndLinesAsync(bool mergeFromPost)
        {
            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;

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
