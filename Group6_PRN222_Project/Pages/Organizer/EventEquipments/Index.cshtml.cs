using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Group6_PRN222_Project.Pages.Organizer.EventEquipments
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<EventEquipment> EventEquipments { get; set; } = default!;

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            var selectedId = HttpContext.Session.GetInt32("SelectedEventId");
            var query = _context.EventEquipments
                .Include(e => e.Equipment)
                .Include(e => e.Event)
                .AsQueryable();

            if (selectedId.HasValue)
            {
                query = query.Where(e => e.EventId == selectedId);
            }
            
            EventEquipments = await query.ToListAsync();
        }
    }
}
