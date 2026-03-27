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
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<FieldReport> Reports { get; set; } = new List<FieldReport>();

        public async System.Threading.Tasks.Task<IActionResult> OnGetAsync()
        {
            if (!SessionHelper.IsOrganizer(HttpContext.Session) && !SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var selectedId = HttpContext.Session.GetInt32("SelectedEventId");

            var query = _context.FieldReports
                .Include(r => r.Event)
                .Include(r => r.Staff)
                .AsQueryable();

            if (selectedId.HasValue)
                query = query.Where(r => r.EventId == selectedId);

            Reports = await query.OrderByDescending(r => r.ReportTime).ToListAsync();
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
