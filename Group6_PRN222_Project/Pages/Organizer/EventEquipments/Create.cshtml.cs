using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Group6_PRN222_Project.Pages.Organizer.EventEquipments
{
    public class CreateModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public CreateModel(ProjectPrn222Context context)
        {
            _context = context;
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

            _context.EventEquipments.Add(EventEquipment);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
