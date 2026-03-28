using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Group6_PRN222_Project.Pages.StaffLogistics.Requests
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

        [BindProperty]
        public int ApproveQty { get; set; }

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

            if (req == null || req.Status != "Pending") return NotFound();

            RequestDetails = req;
            ApproveQty = req.RequestedQuantity; // Default propose to approve all
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync()
        {
            var req = await _context.EventEquipments
                .Include(e => e.Equipment)
                .FirstOrDefaultAsync(e => e.EventId == RequestDetails.EventId && e.EquipmentId == RequestDetails.EquipmentId);

            if (req == null || req.Status != "Pending") return NotFound();

            if (ApproveQty <= 0 || ApproveQty > req.RequestedQuantity)
            {
                ModelState.AddModelError(string.Empty, "Số lượng duyệt không hợp lệ.");
                RequestDetails = req;
                return Page();
            }

            if (ApproveQty > req.Equipment.AvailableQuantity)
            {
                ModelState.AddModelError(string.Empty, "Tồn kho khả dụng không đủ. Hãy tạo phiếu thuê ngoài.");
                RequestDetails = req;
                return Page();
            }

            // Deduct AvailableQuantity
            req.Equipment.AvailableQuantity -= ApproveQty;
            req.ApprovedQuantity = ApproveQty;
            
            if (ApproveQty == req.RequestedQuantity)
            {
                req.Status = "Approved";
            }
            // If partial approval, we could keep it pending? We'll just approve it as is.
            req.Status = "Approved";

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
