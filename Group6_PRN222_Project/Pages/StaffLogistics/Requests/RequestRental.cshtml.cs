using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using System;

namespace Group6_PRN222_Project.Pages.StaffLogistics.Requests
{
    public class RequestRentalModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public RequestRentalModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        [BindProperty]
        public FieldReport RentalRequest { get; set; } = new();

        public Event? EventInfo { get; set; }
        public Models.Equipment? EquipmentInfo { get; set; }
        
        [BindProperty]
        public int RequestedQuantity { get; set; }

        public async Task<IActionResult> OnGetAsync(int eventId, int equipmentId, int gap)
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                HttpContext.Session.SetInt32("UserID", 1);
#else
            if (!SessionHelper.IsLoggedIn(HttpContext.Session) || SessionHelper.GetRole(HttpContext.Session) != RoleConstants.StaffLogistics)
                return RedirectToPage("/Auth/Login");
#endif

            EventInfo = await _context.Events.FindAsync(eventId);
            EquipmentInfo = await _context.Equipments.FindAsync(equipmentId);

            if (EventInfo == null || EquipmentInfo == null) return NotFound();

            RentalRequest.EventId = eventId;
            RequestedQuantity = gap;
            RentalRequest.EstimatePrice = (EquipmentInfo.UnitPrice ?? 0m) * gap;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int eventId, int equipmentId)
        {
            if (!ModelState.IsValid)
            {
                EventInfo = await _context.Events.FindAsync(eventId);
                EquipmentInfo = await _context.Equipments.FindAsync(equipmentId);
                return Page();
            }

            var staffId = SessionHelper.GetUserID(HttpContext.Session) ?? 1;

            var equip = await _context.Equipments.FindAsync(equipmentId);
            string equipName = equip?.EquipmentName ?? "Unknown";

            RentalRequest.StaffId = staffId;
            RentalRequest.EventId = eventId;
            RentalRequest.ReportType = "EquipmentRentalRequest";
            RentalRequest.ReportTime = DateTime.Now;
            RentalRequest.Status = "Submitted";
            
            // Format content to include equipment details
            RentalRequest.Content = $"Thiết bị: {equipName} (ID:{equipmentId})\nSố lượng cần thuê: {RequestedQuantity}\nLý do/Ghi chú: {RentalRequest.Content}";

            _context.FieldReports.Add(RentalRequest);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
