using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Task = System.Threading.Tasks.Task;

namespace Project_PRN222.Pages.StaffMKT.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public IndexModel(ProjectPrn222Context db) => _db = db;

        public List<FieldReport> Reports { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", RoleConstants.StaffMKT);
                HttpContext.Session.SetString("FullName", "Dev Marketing");
                HttpContext.Session.SetString("UserName", "marketing");
            }
#else
            var isLogged = SessionHelper.IsLoggedIn(HttpContext.Session);
            var role = SessionHelper.GetRole(HttpContext.Session);
            if (!isLogged || role != RoleConstants.StaffMKT)
                return RedirectToPage("/Auth/Login");
#endif

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;

            Reports = await _db.FieldReports
                .AsNoTracking()
                .Include(r => r.Event)
                .Where(r => r.StaffId == actorId && r.ReportType == "Marketing")
                .OrderByDescending(r => r.ReportTime)
                .ToListAsync();

            return Page();
        }
    }
}
