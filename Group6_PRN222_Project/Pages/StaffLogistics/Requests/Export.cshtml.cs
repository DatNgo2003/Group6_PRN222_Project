using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Group6_PRN222_Project.Pages.StaffLogistics.Requests
{
    public class ExportModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public ExportModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        [BindProperty]
        public EventEquipment RequestDetails { get; set; } = default!;

        [BindProperty]
        public int ExportQty { get; set; }

        public async Task<IActionResult> OnGetAsync(int eventId, int equipmentId)
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                HttpContext.Session.SetInt32("UserID", 1);
#else
            if (!SessionHelper.IsLoggedIn(HttpContext.Session)) return RedirectToPage("/Auth/Login");
#endif

            var req = await _context.EventEquipments
                .Include(e => e.Equipment)
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.EquipmentId == equipmentId);

            if (req == null || req.Status != "Approved") return NotFound();

            RequestDetails = req;
            ExportQty = req.ApprovedQuantity; // Default propose to export all approved quantity
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var req = await _context.EventEquipments
                .Include(e => e.Equipment)
                .FirstOrDefaultAsync(e => e.EventId == RequestDetails.EventId && e.EquipmentId == RequestDetails.EquipmentId);

            if (req == null || req.Status != "Approved") return NotFound();

            if (ExportQty <= 0 || ExportQty > req.ApprovedQuantity)
            {
                ModelState.AddModelError(string.Empty, "Số lượng xuất không hợp lệ. Phải nhỏ hơn hoặc bằng số lượng đã duyệt.");
                RequestDetails = req;
                return Page();
            }

            req.ExportedQuantity = ExportQty;
            req.Status = "Exported";

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
