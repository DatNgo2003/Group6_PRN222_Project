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

            // Only show departments that have staff users
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
            ViewData["DepartmentList"] = new SelectList(departments, "DepartmentId", "DepartmentName");

            var statuses = new[] { "To Do", "In Progress", "Done", "Completed" };
            ViewData["StatusList"] = new SelectList(statuses);
            return Page();
        }

        // AJAX: GET ?handler=StaffByDept&deptId=X
        public async Task<IActionResult> OnGetStaffByDeptAsync(int deptId)
        {
            var staffRolePrefix = new[] { "Staff" };
            var staff = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.DepartmentId == deptId
                         && u.Role != null
                         && u.Role.RoleName.StartsWith("Staff"))
                .Select(u => new { u.UserId, u.FullName })
                .ToListAsync();
            return new JsonResult(staff);
        }

        [BindProperty]
        public Group6_PRN222_Project.Models.Task Task { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            _context.Tasks.Add(Task);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
