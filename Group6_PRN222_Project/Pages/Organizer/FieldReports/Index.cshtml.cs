using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Project_PRN222.Helpers;

namespace Group6_PRN222_Project.Pages.Organizer.FieldReports
{
    public class MarketingReportDetailVm
    {
        public MarketingFieldReportPayload Payload { get; set; } = new();
        public Dictionary<int, string> TaskNames { get; set; } = new();
    }

    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<FieldReport> Reports { get; set; } = new List<FieldReport>();

        public Dictionary<int, MarketingReportDetailVm> MarketingDetails { get; set; } = new();

        public decimal TotalCost { get; set; }

        public async System.Threading.Tasks.Task<IActionResult> OnGetAsync()
        {
            if (!SessionHelper.IsOrganizer(HttpContext.Session) && !SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var selectedId = HttpContext.Session.GetInt32("SelectedEventId");

            var query = _context.FieldReports
                .Include(r => r.Event)
                .Include(r => r.Staff)
                .Where(r => r.Status != "Draft")
                .AsQueryable();

            if (selectedId.HasValue)
                query = query.Where(r => r.EventId == selectedId);

            Reports = await query.OrderByDescending(r => r.ReportTime).ToListAsync();

            TotalCost = Reports.Where(r => r.EstimatePrice.HasValue).Sum(r => (decimal)r.EstimatePrice!.Value);

            var marketing = Reports.Where(r => r.ReportType == "Marketing").ToList();
            var allTaskIds = marketing
                .SelectMany(r => MarketingFieldReportContent.Parse(r.Content).TaskEstimates.Select(t => t.TaskId))
                .Distinct()
                .ToList();

            var taskNameById = allTaskIds.Count == 0
                ? new Dictionary<int, string>()
                : await _context.Tasks.AsNoTracking()
                    .Where(t => allTaskIds.Contains(t.TaskId))
                    .ToDictionaryAsync(t => t.TaskId, t => t.TaskName ?? $"Task #{t.TaskId}");

            MarketingDetails = new Dictionary<int, MarketingReportDetailVm>();
            foreach (var r in marketing)
            {
                var payload = MarketingFieldReportContent.Parse(r.Content);
                MarketingDetails[r.ReportId] = new MarketingReportDetailVm
                {
                    Payload = payload,
                    TaskNames = payload.TaskEstimates
                        .Select(e => e.TaskId)
                        .Distinct()
                        .ToDictionary(
                            id => id,
                            id => taskNameById.GetValueOrDefault(id, $"Task #{id}"))
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int reportId)
        {
            if (!SessionHelper.IsOrganizer(HttpContext.Session) && !SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var report = await _context.FieldReports.FindAsync(reportId);
            if (report != null)
            {
                report.Status = "Approved";
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Báo cáo #{reportId} đã được duyệt.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy báo cáo.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int reportId)
        {
            if (!SessionHelper.IsOrganizer(HttpContext.Session) && !SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var report = await _context.FieldReports.FindAsync(reportId);
            if (report != null)
            {
                report.Status = "Rejected";
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Báo cáo #{reportId} đã bị từ chối.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy báo cáo.";
            }
            return RedirectToPage();
        }
    }
}
