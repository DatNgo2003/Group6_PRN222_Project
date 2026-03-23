using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
            var selectedId = HttpContext.Session.GetInt32("SelectedEventId");
            var query = _context.DiscountPolicies
                .Include(d => d.Event)
                .AsQueryable();
            if (selectedId.HasValue)
            {
                query = query.Where(d => d.EventId == selectedId);
            }
            Policies = await query.ToListAsync();
        }
    }
}
