using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Organizer.DiscountPolicies
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<DiscountPolicy> Policies { get; set; } = new List<DiscountPolicy>();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            Policies = await _context.DiscountPolicies
                .Include(d => d.Event)
                .ToListAsync();
        }
    }
}
