using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

namespace Group6_PRN222_Project.Pages.Organizer.Contracts
{
    public class EditModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public EditModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Contract Contract { get; set; } = default!;

        public async System.Threading.Tasks.Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FirstOrDefaultAsync(m => m.ContractId == id);
            if (contract == null)
            {
                return NotFound();
            }
            Contract = contract;
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            return Page();
        }

        public async System.Threading.Tasks.Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Contract.Event");

            if (!ModelState.IsValid)
            {
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
                return Page();
            }

            _context.Attach(Contract).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContractExists(Contract.ContractId))
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

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.ContractId == id);
        }
    }
}
