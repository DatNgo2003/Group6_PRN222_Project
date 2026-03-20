using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Organizer.Tasks
{
    public class EditModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public EditModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Group6_PRN222_Project.Models.Task Task { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Tasks == null)
            {
                return NotFound();
            }

            var task =  await _context.Tasks.FirstOrDefaultAsync(m => m.TaskId == id);
            if (task == null)
            {
                return NotFound();
            }
            Task = task;
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName");
            var staffRoles = new[] { "Security", "MKT", "Marketing", "Logistics", "Staff" };
            var staffUsers = await _context.Users.Include(u => u.Role)
                .Where(u => u.Role != null && staffRoles.Contains(u.Role.RoleName)).ToListAsync();
            
            ViewData["AssignedTo"] = new SelectList(staffUsers, "UserId", "FullName");
            var statuses = new[] { "To Do", "In Progress", "Done", "Completed" };
            ViewData["StatusList"] = new SelectList(statuses);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _context.Attach(Task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(Task.TaskId))
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

        private bool TaskExists(int id)
        {
          return (_context.Tasks?.Any(e => e.TaskId == id)).GetValueOrDefault();
        }
    }
}
