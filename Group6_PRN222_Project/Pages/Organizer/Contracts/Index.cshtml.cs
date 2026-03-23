using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Group6_PRN222_Project.Pages.Organizer.Contracts
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<Contract> Contracts { get; set; } = default!;

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            var selectedId = HttpContext.Session.GetInt32("SelectedEventId");
            var query = _context.Contracts
                .Include(c => c.Event)
                .AsQueryable();

            if (selectedId.HasValue)
            {
                query = query.Where(c => c.EventId == selectedId);
            }
            
            Contracts = await query.ToListAsync();
        }
    }
}
