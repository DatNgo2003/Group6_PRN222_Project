using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Pages.Events
{
    public class EventDetailModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        public EventDetailModel(ProjectPrn222Context db) => _db = db;

        public Event? Event { get; set; }
        public int TicketCount { get; set; }
        public bool AlreadyJoined { get; set; }

        [BindProperty] public string PaymentMethod { get; set; } = "Direct";

        public async System.Threading.Tasks.Task<IActionResult> OnGetAsync(int id)
        {
            Event = await _db.Events
                .Include(e => e.Organizer)
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (Event == null) return NotFound();

            TicketCount = Event.Tickets.Count;

            // Kiểm tra user đã đăng ký chưa
            var userId = HttpContext.Session.GetInt32("userid");
            if (userId.HasValue)
            {
                var participant = await _db.Participants
                    .FirstOrDefaultAsync(p => p.Email == HttpContext.Session.GetString("username"));

                if (participant != null)
                    AlreadyJoined = await _db.Tickets
                        .AnyAsync(t => t.EventId == id && t.ParticipantId == participant.ParticipantId);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostRegisterAsync(int id, string paymentMethod)
        {
            // Kiểm tra đã login chưa
            var userId = HttpContext.Session.GetInt32("userid");
            if (!userId.HasValue)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đăng ký tham gia sự kiện.";
                return RedirectToPage("/Account/Login", new { returnUrl = $"/Events/EventDetail/{id}" });
            }

            // Kiểm tra role phải là Participant
            var role = HttpContext.Session.GetString("role") ?? "";
            if (role != "Participant" && role != "")
            {
                TempData["Error"] = "Chỉ tài khoản Participant mới có thể đăng ký tham gia sự kiện.";
                return RedirectToPage(new { id });
            }

            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            // Tìm hoặc tạo Participant từ thông tin session
            var email = HttpContext.Session.GetString("username") ?? "";
            var fullName = HttpContext.Session.GetString("fullname") ?? "";

            var participant = await _db.Participants.FirstOrDefaultAsync(p => p.Email == email);
            if (participant == null)
            {
                participant = new Participant
                {
                    FullName = fullName,
                    Email = email,
                    IsVip = false,
                    IsBlacklisted = false,
                    Status = "Active"
                };
                _db.Participants.Add(participant);
                await _db.SaveChangesAsync();
            }

            // Kiểm tra đã đăng ký chưa
            bool alreadyJoined = await _db.Tickets
                .AnyAsync(t => t.EventId == id && t.ParticipantId == participant.ParticipantId);

            if (alreadyJoined)
            {
                TempData["Error"] = "Bạn đã đăng ký sự kiện này rồi.";
                return RedirectToPage(new { id });
            }

            // Xác định trạng thái thanh toán theo phương thức
            var paymentStatus = paymentMethod switch
            {
                "Direct" => "Pending",      // Trực tiếp → chờ xác nhận tại quầy
                "Transfer" => "Pending",      // Chuyển khoản → chờ xác nhận
                "CreditCard" => "Paid",         // Thẻ tín dụng → thanh toán ngay
                "ZaloPay" => "Paid",         // ZaloPay → thanh toán ngay
                _ => "Pending"
            };

            // Tạo ticket
            var ticket = new Ticket
            {
                EventId = id,
                ParticipantId = participant.ParticipantId,
                Qrcode = Guid.NewGuid(),
                PaymentStatus = paymentStatus,
                Status = "Active"
            };
            _db.Tickets.Add(ticket);

            // Ghi audit log
            _db.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId = userId,
                Action = $"REGISTER_EVENT_{id}_PAYMENT_{paymentMethod.ToUpper()}",
                TableName = "Tickets",
                ActionTime = DateTime.Now,
                Status = "Success"
            });

            await _db.SaveChangesAsync();

            var msg = paymentMethod switch
            {
                "Direct" => "Đăng ký thành công! Vui lòng thanh toán trực tiếp tại quầy vào ngày sự kiện.",
                "Transfer" => "Đăng ký thành công! Vui lòng chuyển khoản và chờ xác nhận từ ban tổ chức.",
                "CreditCard" => "Đăng ký và thanh toán thành công! Vé của bạn đã được xác nhận.",
                "ZaloPay" => "Đăng ký và thanh toán ZaloPay thành công! Vé của bạn đã được xác nhận.",
                _ => "Đăng ký thành công!"
            };

            TempData["Success"] = msg;
            return RedirectToPage(new { id });
        }
    }
}
