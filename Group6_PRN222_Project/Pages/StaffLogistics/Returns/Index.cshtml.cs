using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Group6_PRN222_Project.Pages.StaffLogistics.Returns
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<EventEquipment> ExportedEquipments { get; set; } = default!;
        public IList<ReturnLog> RecentLogs { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                HttpContext.Session.SetInt32("UserID", 1);
#else
            if (!SessionHelper.IsLoggedIn(HttpContext.Session) || SessionHelper.GetRole(HttpContext.Session) != RoleConstants.StaffLogistics)
                return RedirectToPage("/Auth/Login");
#endif

            ExportedEquipments = await _context.EventEquipments
                .Include(e => e.Equipment)
                .Include(e => e.Event)
                .Where(e => e.Status == "Exported")
                .OrderByDescending(e => e.Event.EndDate)
                .ToListAsync();
                
            RecentLogs = await _context.ReturnLogs
                .Include(l => l.Event)
                .Include(l => l.Equipment)
                .OrderByDescending(l => l.CheckDate)
                .Take(20)
                .ToListAsync();

            return Page();
        }
    }
}
