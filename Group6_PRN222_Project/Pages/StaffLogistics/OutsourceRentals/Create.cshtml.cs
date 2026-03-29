using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Group6_PRN222_Project.Pages.StaffLogistics.OutsourceRentals
{
    public class CreateModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public CreateModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        [BindProperty]
        public OutsourceRental OutsourceRental { get; set; } = default!;

        public SelectList EventsList { get; set; } = default!;
        public SelectList VendorsList { get; set; } = default!;
        public SelectList EquipmentList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? eventId, int? equipmentId, int? gap)
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                HttpContext.Session.SetInt32("UserID", 1);
#else
            if (!SessionHelper.IsLoggedIn(HttpContext.Session) || SessionHelper.GetRole(HttpContext.Session) != RoleConstants.StaffLogistics)
                return RedirectToPage("/Auth/Login");
#endif

            OutsourceRental = new OutsourceRental
            {
                EventId = eventId,
                EquipmentId = equipmentId ?? 0,
                RentQuantity = gap ?? 1,
                ExpectedReturnDate = DateTime.Now.AddDays(7)
            };

            await LoadSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("OutsourceRental.Equipment");
            ModelState.Remove("OutsourceRental.Vendor");
            ModelState.Remove("OutsourceRental.Event");

            if (!ModelState.IsValid || OutsourceRental.RentQuantity <= 0)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            OutsourceRental.Status = "Pending";
            _context.OutsourceRentals.Add(OutsourceRental);
            await _context.SaveChangesAsync();

            // After creating, redirect back to Requests if query came from there? 
            // Better to go to OutsourceRentals Index.
            return RedirectToPage("./Index");
        }

        private async Task LoadSelectListsAsync()
        {
            var events = await _context.Events.OrderByDescending(e => e.StartDate).ToListAsync();
            EventsList = new SelectList(events, "EventId", "EventName");

            var vendors = await _context.Vendors.ToListAsync();
            VendorsList = new SelectList(vendors, "VendorId", "VendorName");

            var equipments = await _context.Equipments.ToListAsync();
            EquipmentList = new SelectList(equipments, "EquipmentId", "EquipmentName");
        }
    }
}
