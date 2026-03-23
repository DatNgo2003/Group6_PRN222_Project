using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Group6_PRN222_Project.Pages.Organizer.Contracts
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
            return Page();
        }

        [BindProperty]
        public Contract Contract { get; set; } = default!;

        public async System.Threading.Tasks.Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Contract.Event");

            if (!ModelState.IsValid)
            {
                var events = await _context.Events.ToListAsync();
                ViewData["EventId"] = new SelectList(events, "EventId", "EventName");
                return Page();
            }

            _context.Contracts.Add(Contract);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
