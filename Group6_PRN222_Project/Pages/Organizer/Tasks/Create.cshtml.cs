using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Organizer.Tasks
{
    public class CreateModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public CreateModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName");
            // Optionally, only show users with Staff roles
            var staffRoles = new[] { "Security", "MKT", "Marketing", "Logistics", "Staff" };
            var staffUsers = await _context.Users.Include(u => u.Role)
                .Where(u => u.Role != null && staffRoles.Contains(u.Role.RoleName)).ToListAsync();
            
            ViewData["AssignedTo"] = new SelectList(staffUsers, "UserId", "FullName");
            var statuses = new[] { "To Do", "In Progress", "Done", "Completed" };
            ViewData["StatusList"] = new SelectList(statuses);
            return Page();
        }

        [BindProperty]
        public EventTask Task { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            _context.Tasks.Add(Task);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
