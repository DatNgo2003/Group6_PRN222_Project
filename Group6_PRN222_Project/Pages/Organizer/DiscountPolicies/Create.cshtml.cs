using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Organizer.DiscountPolicies
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
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            return Page();
        }

        [BindProperty]
        public DiscountPolicy DiscountPolicy { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            _context.DiscountPolicies.Add(DiscountPolicy);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
