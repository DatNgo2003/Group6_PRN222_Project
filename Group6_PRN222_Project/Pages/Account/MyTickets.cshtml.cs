using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Pages.Account
{
    public class MyTicketsModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        public MyTicketsModel(ProjectPrn222Context db) => _db = db;

        public List<Ticket> Tickets { get; set; } = new();
        public Participant? CurrentParticipant { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterStatus { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Ph?i ??ng nh?p
            var userId = HttpContext.Session.GetInt32("userid");
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login",
                    new { returnUrl = "/Account/MyTickets" });

            // Ph?i là Participant
            var role = HttpContext.Session.GetString("role") ?? "";
            if (role != "Participant")
            {
                TempData["Error"] = "Trang này ch? dành cho tài kho?n Participant.";
                return RedirectToPage("/Index");
            }

            // Tìm Participant theo email session
            CurrentParticipant = await _db.Participants
    .FirstOrDefaultAsync(p => p.UserId == userId);

            if (CurrentParticipant == null)
            {
                TempData["Error"] = "Không tìm th?y thông tin ng??i tham d?.";
                return RedirectToPage("/Index");
            }

            // Query vé
            var query = _db.Tickets
                .Include(t => t.Event)
                    .ThenInclude(e => e!.Organizer)
                .Where(t => t.ParticipantId == CurrentParticipant.ParticipantId);

            if (!string.IsNullOrEmpty(FilterStatus))
                query = query.Where(t => t.Status == FilterStatus);

            Tickets = await query
                .OrderByDescending(t => t.Event!.StartDate)
                .ToListAsync();

            return Page();
        }

        // POST: H?y vé
        public async Task<IActionResult> OnPostCancelAsync(int ticketId)
        {
            var userId = HttpContext.Session.GetInt32("userid");
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            // Tìm Participant c?a user hi?n t?i
            var participant = await _db.Participants
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
            {
                TempData["Error"] = "Không tìm th?y thông tin ng??i tham d?.";
                return RedirectToPage();
            }

            // Tìm vé — ph?i thu?c v? Participant này
            var ticket = await _db.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId
                                        && t.ParticipantId == participant.ParticipantId);

            if (ticket == null)
            {
                TempData["Error"] = "Không tìm th?y vé.";
                return RedirectToPage();
            }

            // Ch? h?y ???c vé Valid và s? ki?n ch?a di?n ra
            if (ticket.Status != "Valid")
            {
                TempData["Error"] = "Ch? có th? h?y vé ?ang ? tr?ng thái Valid.";
                return RedirectToPage();
            }

            if (ticket.Event?.StartDate.HasValue == true
                && ticket.Event.StartDate.Value <= DateTime.Now)
            {
                TempData["Error"] = "Không th? h?y vé c?a s? ki?n ?ã b?t ??u ho?c k?t thúc.";
                return RedirectToPage();
            }

            ticket.Status = "Cancelled";

            _db.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId = userId,
                Action = $"CANCEL_TICKET_{ticketId}_EVENT_{ticket.EventId}",
                TableName = "Tickets",
                ActionTime = DateTime.Now,
                Status = "Success"
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "H?y vé thành công.";
            return RedirectToPage();
        }
    }
}