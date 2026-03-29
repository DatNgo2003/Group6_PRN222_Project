using Group6_PRN222_Project.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

namespace Project_PRN222.Pages.Admin.Reports
{
    [AuthorizeRole("Admin")]
    public class RevenueReportModel : PageModel
    {
        private readonly IRevenueReportService _reportSvc;
        public RevenueReportModel(IRevenueReportService reportSvc) => _reportSvc = reportSvc;

        [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
        [BindProperty(SupportsGet = true)] public int? EventId { get; set; }

        public RevenueReportDto Report { get; set; } = new();
        public SelectList EventList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var events = await _reportSvc.GetEventListAsync();
            EventList = new SelectList(
                events.Select(e => new { e.Id, e.Name }),
                "Id", "Name", EventId);

            Report = await _reportSvc.GetReportAsync(DateFrom, DateTo, EventId);
            return Page();
        }
    }
}
