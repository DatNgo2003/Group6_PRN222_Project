using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Organizer.Events
{
    public class CreateModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public CreateModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["OrganizerId"] = new SelectList(_context.Users, "UserId", "FullName");
            var statuses = new[] { "Planning", "Ongoing", "Completed", "Cancelled" };
            ViewData["StatusList"] = new SelectList(statuses);
            return Page();
        }

        [BindProperty]
        public Event Event { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            _context.Events.Add(Event);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
