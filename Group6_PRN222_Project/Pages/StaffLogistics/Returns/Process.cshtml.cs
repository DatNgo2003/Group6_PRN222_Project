using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Group6_PRN222_Project.Pages.StaffLogistics.Returns
{
    public class ProcessModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public ProcessModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        [BindProperty]
        public EventEquipment RequestDetails { get; set; } = default!;

        [BindProperty] public int GoodQty { get; set; }
        [BindProperty] public int DefectiveQty { get; set; }
        [BindProperty] public int LostQty { get; set; }
        [BindProperty] public string? Note { get; set; }

        public async Task<IActionResult> OnGetAsync(int eventId, int equipmentId)
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                HttpContext.Session.SetInt32("UserID", 1);
#else
            if (!SessionHelper.IsLoggedIn(HttpContext.Session) || SessionHelper.GetRole(HttpContext.Session) != RoleConstants.StaffLogistics)
                return RedirectToPage("/Auth/Login");
#endif

            var req = await _context.EventEquipments
                .Include(e => e.Equipment)
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.EquipmentId == equipmentId);

            if (req == null || req.Status != "Exported") return NotFound();

            RequestDetails = req;
            GoodQty = req.ExportedQuantity; // Default propose all are good
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var req = await _context.EventEquipments
                .Include(e => e.Equipment)
                .FirstOrDefaultAsync(e => e.EventId == RequestDetails.EventId && e.EquipmentId == RequestDetails.EquipmentId);

            if (req == null || req.Status != "Exported") return NotFound();

            var totalReturned = GoodQty + DefectiveQty + LostQty;
            if (totalReturned != req.ExportedQuantity)
            {
                ModelState.AddModelError(string.Empty, "Tổng số lượng Tốt + Hỏng + Cần đền bù phải bằng đúng số lượng đã Xuất Kho.");
                RequestDetails = req;
                return Page();
            }

            // Update Equipment Quantities
            req.Equipment.AvailableQuantity += GoodQty;
            req.Equipment.DefectiveQuantity += DefectiveQty;
            req.Equipment.TotalOwned -= LostQty;

            // Record ReturnLog
            var rLog = new ReturnLog
            {
                EventId = req.EventId,
                EquipmentId = req.EquipmentId,
                ReturnedQuantity = GoodQty,
                DefectiveQuantity = DefectiveQty,
                LostQuantity = LostQty,
                CheckDate = DateTime.Now,
                Note = Note ?? ""
            };

            _context.ReturnLogs.Add(rLog);

            req.Status = "Completed";

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
