using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Group6_PRN222_Project.Pages.StaffLogistics.OutsourceRentals
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<OutsourceRental> Rentals { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                HttpContext.Session.SetInt32("UserID", 1);
#else
            if (!SessionHelper.IsLoggedIn(HttpContext.Session) || SessionHelper.GetRole(HttpContext.Session) != RoleConstants.StaffLogistics)
                return RedirectToPage("/Auth/Login");
#endif

            Rentals = await _context.OutsourceRentals
                .Include(r => r.Equipment)
                .Include(r => r.Event)
                .Include(r => r.Vendor)
                .OrderByDescending(r => r.RentalId)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostReceiveAsync(int id)
        {
            var rental = await _context.OutsourceRentals
                .Include(r => r.Equipment)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null || rental.Status != "Pending") return NotFound();

            rental.Status = "Received";
            rental.Equipment.AvailableQuantity += rental.RentQuantity;
            rental.Equipment.OutsourcedQuantity += rental.RentQuantity;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
        
        public async Task<IActionResult> OnPostReturnAsync(int id)
        {
            var rental = await _context.OutsourceRentals
                .Include(r => r.Equipment)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null || rental.Status != "Received") return NotFound();

            rental.Status = "Returned";
            rental.Equipment.AvailableQuantity -= rental.RentQuantity;
            rental.Equipment.OutsourcedQuantity -= rental.RentQuantity;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
