using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Project_PRN222.Pages.StaffLogistics.Equipment
{
    public class EditModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public EditModel(ProjectPrn222Context db) => _db = db;

        [BindProperty]
        public Group6_PRN222_Project.Models.Equipment Equipment { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", RoleConstants.StaffLogistics);
            }
#else
            var role = SessionHelper.GetRole(HttpContext.Session);
            if (role != RoleConstants.StaffLogistics) return RedirectToPage("/Auth/Login");
#endif

            var eq = await _db.Equipments.FirstOrDefaultAsync(e => e.EquipmentId == id);
            if (eq == null) return NotFound();

            Equipment = eq;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var dbEq = await _db.Equipments.FirstOrDefaultAsync(e => e.EquipmentId == Equipment.EquipmentId);
            if (dbEq == null) return NotFound();

            // Calculate deltas
            int totalOwnedDelta = Equipment.TotalOwned - dbEq.TotalOwned;
            int defectiveDelta = Equipment.DefectiveQuantity - dbEq.DefectiveQuantity;
            
            dbEq.EquipmentName = Equipment.EquipmentName;
            dbEq.UnitPrice = Equipment.UnitPrice;
            dbEq.TotalOwned = Equipment.TotalOwned;
            dbEq.DefectiveQuantity = Equipment.DefectiveQuantity;

            // Adjust AvailableQuantity based on both changes
            // NewAvailable = OldAvailable + (Change in TotalOwned) - (Change in Defective)
            dbEq.AvailableQuantity += (totalOwnedDelta - defectiveDelta);
            
            if (dbEq.AvailableQuantity < 0) dbEq.AvailableQuantity = 0;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật thiết bị {dbEq.EquipmentName}.";
            return RedirectToPage("./Index");
        }
    }
}
