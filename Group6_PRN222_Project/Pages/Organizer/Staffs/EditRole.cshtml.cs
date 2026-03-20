using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Organizer.Staffs
{
    public class EditRoleModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public EditRoleModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        [BindProperty]
        public int TargetUserId { get; set; }

        [BindProperty]
        public int? SelectedRoleId { get; set; }

        public string StaffFullName { get; set; }
        public string StaffEmail { get; set; }

        public SelectList RolesList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            TargetUserId = user.UserId;
            SelectedRoleId = user.RoleId;
            StaffFullName = user.FullName;
            StaffEmail = user.Email;

            // Only allow assigning specific roles (Security, MKT, Logistics)
            var allowedRoleNames = new[] { "Security", "MKT", "Marketing", "Logistics", "Staff" };
            var roles = await _context.Roles
                .Where(r => allowedRoleNames.Contains(r.RoleName) || r.RoleId == user.RoleId)
                .ToListAsync();

            RolesList = new SelectList(roles, "RoleId", "RoleName");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userToUpdate = await _context.Users.FindAsync(TargetUserId);
            if (userToUpdate == null)
            {
                return NotFound();
            }

            userToUpdate.RoleId = SelectedRoleId;
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
