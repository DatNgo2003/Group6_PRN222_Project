using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_PRN222.Helpers;

namespace Project_PRN222.Pages.StaffLogistics.Equipment
{
    public class CreateModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public CreateModel(ProjectPrn222Context db) => _db = db;

        [BindProperty]
        public Group6_PRN222_Project.Models.Equipment Equipment { get; set; } = new();

        public IActionResult OnGet()
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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Initialize quantities
            Equipment.AvailableQuantity = Equipment.TotalOwned;
            Equipment.DefectiveQuantity = 0;
            Equipment.OutsourcedQuantity = 0;

            _db.Equipments.Add(Equipment);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm thiết bị {Equipment.EquipmentName} vào kho.";
            return RedirectToPage("./Index");
        }
    }
}
