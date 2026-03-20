using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Organizer.Staffs
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<User> StaffUsers { get; set; } = new List<User>();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            StaffUsers = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .ToListAsync();
        }
    }
}
