using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

namespace Group6_PRN222_Project.Pages.Organizer.EventEquipments
{
    public class EditModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public EditModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        [BindProperty]
        public EventEquipment EventEquipment { get; set; } = default!;

        public async System.Threading.Tasks.Task<IActionResult> OnGetAsync(int? eventId, int? equipmentId)
        {
            if (eventId == null || equipmentId == null)
            {
                return NotFound();
            }

            var eventequipment = await _context.EventEquipments
                .Include(e => e.Equipment)
                .FirstOrDefaultAsync(m => m.EventId == eventId && m.EquipmentId == equipmentId);
                
            if (eventequipment == null)
            {
                return NotFound();
            }
            EventEquipment = eventequipment;
            return Page();
        }

        public async System.Threading.Tasks.Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("EventEquipment.Event");
            ModelState.Remove("EventEquipment.Equipment");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(EventEquipment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventEquipmentExists(EventEquipment.EventId, EventEquipment.EquipmentId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool EventEquipmentExists(int eventId, int equipmentId)
        {
            return _context.EventEquipments.Any(e => e.EventId == eventId && e.EquipmentId == equipmentId);
        }
    }
}
