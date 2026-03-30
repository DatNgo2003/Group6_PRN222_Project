using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Project_PRN222.Services;

namespace Group6_PRN222_Project.Pages.Organizer.EventEquipments
{
    public class CreateModel : PageModel
    {
        private readonly ProjectPrn222Context _context;
        private readonly IEquipmentService _equipmentService;

        public CreateModel(ProjectPrn222Context context, IEquipmentService equipmentService)
        {
            _context = context;
            _equipmentService = equipmentService;
        }

        public async System.Threading.Tasks.Task<IActionResult> OnGetAsync()
        {
            var selectedEventId = HttpContext.Session.GetInt32("SelectedEventId");
            var events = await _context.Events.ToListAsync();
            ViewData["EventId"] = new SelectList(events, "EventId", "EventName", selectedEventId);
            ViewData["EquipmentId"] = new SelectList(await _context.Equipments.ToListAsync(), "EquipmentId", "EquipmentName");
            return Page();
        }

        [BindProperty]
        public EventEquipment EventEquipment { get; set; } = default!;

        public async System.Threading.Tasks.Task<IActionResult> OnPostAsync()
        {
            // Optional: Ignore navigation property validation
            ModelState.Remove("EventEquipment.Event");
            ModelState.Remove("EventEquipment.Equipment");

            if (!ModelState.IsValid)
            {
                var events = await _context.Events.ToListAsync();
                ViewData["EventId"] = new SelectList(events, "EventId", "EventName");
                ViewData["EquipmentId"] = new SelectList(await _context.Equipments.ToListAsync(), "EquipmentId", "EquipmentName");
                return Page();
            }

            var exists = await _context.EventEquipments.AnyAsync(e => e.EventId == EventEquipment.EventId && e.EquipmentId == EventEquipment.EquipmentId);
            if (exists)
            {
                ModelState.AddModelError(string.Empty, "This requirement is already added for this event.");
                var events = await _context.Events.ToListAsync();
                ViewData["EventId"] = new SelectList(events, "EventId", "EventName");
                ViewData["EquipmentId"] = new SelectList(await _context.Equipments.ToListAsync(), "EquipmentId", "EquipmentName");
                return Page();
            }

            var eventData = await _context.Events.FindAsync(EventEquipment.EventId);
            if (eventData != null && eventData.StartDate.HasValue && eventData.EndDate.HasValue)
            {
                var dynamicAvailable = await _equipmentService.GetAvailableQuantityAsync(
                    EventEquipment.EquipmentId, 
                    eventData.StartDate.Value, 
                    eventData.EndDate.Value
                );

                if (EventEquipment.RequestedQuantity > dynamicAvailable)
                {
                    ModelState.AddModelError(string.Empty, $"Warning: Requested quantity ({EventEquipment.RequestedQuantity}) exceeds estimated availability ({dynamicAvailable}) for this event's timeframe.");
                }
            }

            EventEquipment.ApprovedQuantity = 0;
            EventEquipment.ExportedQuantity = 0;
            EventEquipment.Status = "Pending";

            _context.EventEquipments.Add(EventEquipment);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
