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
                return NotFound();

            var task = await _context.Tasks.FirstOrDefaultAsync(m => m.TaskId == id);
            if (task == null)
                return NotFound();

            Task = task;

            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", task.EventId);

            // Departments — only those with Staff users
            var staffDeptIds = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.DepartmentId != null && u.Role != null && u.Role.RoleName.StartsWith("Staff"))
                .Select(u => u.DepartmentId!.Value)
                .Distinct()
                .ToListAsync();
            var departments = await _context.Departments
                .Where(d => staffDeptIds.Contains(d.DepartmentId))
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            // Determine current department of the assigned staff (if any)
            int? currentDeptId = null;
            if (task.AssignedTo.HasValue)
            {
                var assignedUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == task.AssignedTo.Value);
                currentDeptId = assignedUser?.DepartmentId;
            }

            ViewData["DepartmentList"] = new SelectList(departments, "DepartmentId", "DepartmentName", currentDeptId);
            ViewData["CurrentDeptId"] = currentDeptId;

            // Pre-load staff of current department
            List<User> staffUsers = new();
            if (currentDeptId.HasValue)
            {
                staffUsers = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.DepartmentId == currentDeptId.Value
                             && u.Role != null
                             && u.Role.RoleName.StartsWith("Staff"))
                    .ToListAsync();
            }
            ViewData["AssignedTo"] = new SelectList(staffUsers, "UserId", "FullName", task.AssignedTo);

            var statuses = new[] { "To Do", "In Progress", "Done", "Completed" };
            ViewData["StatusList"] = new SelectList(statuses, task.Status);
            return Page();
        }

        // AJAX: GET ?handler=StaffByDept&deptId=X
        public async Task<IActionResult> OnGetStaffByDeptAsync(int deptId)
        {
            var staff = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.DepartmentId == deptId
                         && u.Role != null
                         && u.Role.RoleName.StartsWith("Staff"))
                .Select(u => new { u.UserId, u.FullName })
                .ToListAsync();
            return new JsonResult(staff);
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
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("./Index");
        }

        private bool TaskExists(int id)
        {
            return (_context.Tasks?.Any(e => e.TaskId == id)).GetValueOrDefault();
        }
    }
}
