using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

namespace Group6_PRN222_Project.Pages.Organizer.EventEquipments
{
    public class DeleteModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public DeleteModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        [BindProperty]
        public EventEquipment EventEquipment { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? eventId, int? equipmentId)
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

            if (eventequipment.Status != "Pending")
            {
                // Can't delete if not pending
                return RedirectToPage("./Index");
            }

            EventEquipment = eventequipment;
            return Page();
        }

        public async System.Threading.Tasks.Task<IActionResult> OnPostAsync(int? eventId, int? equipmentId)
        {
            if (eventId == null || equipmentId == null)
            {
                return NotFound();
            }

            var eventequipment = await _context.EventEquipments.FindAsync(eventId, equipmentId);

            if (eventequipment != null)
            {
                EventEquipment = eventequipment;
                _context.EventEquipments.Remove(EventEquipment);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
