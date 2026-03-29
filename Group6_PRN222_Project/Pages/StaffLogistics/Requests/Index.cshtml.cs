using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Group6_PRN222_Project.Pages.StaffLogistics.Requests
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<EventEquipment> Requests { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", RoleConstants.StaffLogistics);
                HttpContext.Session.SetString("FullName", "Dev Logistics");
                HttpContext.Session.SetString("UserName", "logistics");
            }
#else
            if (!SessionHelper.IsLoggedIn(HttpContext.Session) || SessionHelper.GetRole(HttpContext.Session) != RoleConstants.StaffLogistics)
                return RedirectToPage("/Auth/Login");
#endif

            Requests = await _context.EventEquipments
                .Include(e => e.Equipment)
                .Include(e => e.Event)
                .Where(e => e.Status == "Pending" || e.Status == "Approved")
                .OrderByDescending(e => e.Event.StartDate)
                .ToListAsync();

            return Page();
        }
    }
}
