using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Project_PRN222.Pages.StaffLogistics.Equipment
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public IndexModel(ProjectPrn222Context db) => _db = db;

        public List<Group6_PRN222_Project.Models.Equipment> Equipments { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }

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
            var isLogged = SessionHelper.IsLoggedIn(HttpContext.Session);
            var role = SessionHelper.GetRole(HttpContext.Session);
            if (!isLogged || role != RoleConstants.StaffLogistics)
                return RedirectToPage("/Auth/Login");
#endif

            var query = _db.Equipments.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(FilterStatus) &&
                !string.Equals(FilterStatus, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(e => e.Status == FilterStatus);
            }

            Equipments = await query
                .OrderBy(e => e.EquipmentId)
                .ToListAsync();

            return Page();
        }
    }
}

