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
        // POST: Hủy vé
        public async Task<IActionResult> OnPostCancelAsync(int ticketId)
        {
            var userId = HttpContext.Session.GetInt32("userid");
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            // Tìm Participant của user hiện tại
            var participant = await _db.Participants
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người tham dự.";
                return RedirectToPage();
            }

            // Tìm vé — phải thuộc về Participant này
            var ticket = await _db.Tickets
                .FirstOrDefaultAsync(t => t.TicketId == ticketId
                                        && t.ParticipantId == participant.ParticipantId);

            if (ticket == null)
            {
                TempData["Error"] = "Không tìm thấy vé.";
                return RedirectToPage();
            }

            // Chỉ hủy được vé chưa thanh toán
            if (ticket.PaymentStatus == "Paid")
            {
                TempData["Error"] = "Không thể hủy vé đã thanh toán.";
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

            TempData["Success"] = "Hủy vé thành công.";
            return RedirectToPage();
        }
    }
}