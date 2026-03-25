using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

namespace Group6_PRN222_Project.Pages.Organizer.Tickets
{
    public class EditModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public EditModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Ticket Ticket { get; set; } = default!;

        [BindProperty]
        public int? DiscountPolicyId { get; set; }

        public SelectList EventList { get; set; } = default!;
        public SelectList ParticipantList { get; set; } = default!;
        public SelectList DiscountPolicyList { get; set; } = default!;

        public async System.Threading.Tasks.Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FirstOrDefaultAsync(m => m.TicketId == id);
            if (ticket == null)
            {
                return NotFound();
            }
            Ticket = ticket;

            EventList = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName");
            ParticipantList = new SelectList(await _context.Participants.ToListAsync(), "ParticipantId", "FullName");
            var policies = await _context.DiscountPolicies.Where(d => d.Status == "Active").ToListAsync();
            DiscountPolicyList = new SelectList(policies, "PolicyId", "PolicyName");

            return Page();
        }

        public async System.Threading.Tasks.Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Ticket.Event");
            ModelState.Remove("Ticket.Participant");

            // Recalculate ticket price
            var ev = await _context.Events.FindAsync(Ticket.EventId);
            decimal ticketPrice = ev?.Amount ?? 0;

            if (DiscountPolicyId.HasValue)
            {
                var policy = await _context.DiscountPolicies.FindAsync(DiscountPolicyId.Value);
                if (policy != null && policy.DiscountPercent.HasValue)
                {
                    ticketPrice = ticketPrice - (ticketPrice * policy.DiscountPercent.Value / 100);
                }
            }

            Ticket.PaymentStatus = ticketPrice.ToString("N0");

            _context.Attach(Ticket).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tickets.Any(e => e.TicketId == Ticket.TicketId))
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
    }
}
