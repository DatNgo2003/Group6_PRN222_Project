using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Project_PRN222.Services;

namespace Group6_PRN222_Project.Pages.StaffLogistics.Requests
{
    public class ProcessModel : PageModel
    {
        private readonly ProjectPrn222Context _context;
        private readonly IEquipmentService _equipmentService;

        public ProcessModel(ProjectPrn222Context context, IEquipmentService equipmentService)
        {
            _context = context;
            _equipmentService = equipmentService;
        }

        [BindProperty]
        public EventEquipment RequestDetails { get; set; } = default!;

        [BindProperty]
        public int ApproveQty { get; set; }

        public int DynamicAvailableQuantity { get; set; }

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

            DynamicAvailableQuantity = await _equipmentService.GetAvailableQuantityAsync(
                req.EquipmentId, 
                req.Event.StartDate ?? DateTime.Now, 
                req.Event.EndDate ?? DateTime.Now.AddHours(1)
            );

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync()
        {
            var req = await _context.EventEquipments
                .Include(e => e.Equipment)
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EventId == RequestDetails.EventId && e.EquipmentId == RequestDetails.EquipmentId);

            if (req == null || req.Status != "Pending") return NotFound();

            if (ApproveQty <= 0 || ApproveQty > req.RequestedQuantity)
            {
                ModelState.AddModelError(string.Empty, "Số lượng duyệt không hợp lệ.");
                RequestDetails = req;
                return Page();
            }

            var dynamicAvailable = await _equipmentService.GetAvailableQuantityAsync(
                req.EquipmentId, 
                req.Event.StartDate ?? DateTime.Now, 
                req.Event.EndDate ?? DateTime.Now.AddHours(1)
            );

            if (ApproveQty > dynamicAvailable)
            {
                ModelState.AddModelError(string.Empty, $"Tồn kho khả dụng cho thời gian này không đủ (Chỉ còn {dynamicAvailable}). Hãy tạo phiếu thuê ngoài.");
                RequestDetails = req;
                return Page();
            }

            // Physical AvailableQuantity is NOT deducted on approval, only on Export (Xuất kho).
            // Approval only confirms the 'Reserved' quantity for the schedule.
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
