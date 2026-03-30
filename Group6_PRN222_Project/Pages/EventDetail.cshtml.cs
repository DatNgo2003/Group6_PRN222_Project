using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParticipantModel = Group6_PRN222_Project.Models.Participant;

namespace Group6_PRN222_Project.Pages.Events
{
    
    public class EventDetailModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        public EventDetailModel(ProjectPrn222Context db) => _db = db;

        // ── Display data ──────────────────────────────────────────────
        public Event? Event { get; set; }
        public int TicketCount { get; set; }
        public bool AlreadyJoined { get; set; }
        public bool IsOpenForReg { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool IsParticipant { get; set; }
        public List<DiscountPolicy> DiscountPolicies { get; set; } = new();

        // ── Ticket pricing ────────────────────────────────────────────
        public decimal PriceStandard => Event?.Amount ?? 0;
        public decimal PriceVip => (Event?.Amount ?? 0) * 1.5m;
        public decimal PriceStudent => (Event?.Amount ?? 0) * 0.7m;
        public decimal PriceComplimentary => 0;

        // ── Form bindings ─────────────────────────────────────────────
        [BindProperty] public string PaymentMethod { get; set; } = "Direct";
        [BindProperty] public string TicketType { get; set; } = "Standard";

        // Sự kiện "Open" cho đăng ký
        private const string OpenStatus = "Open";

        public async System.Threading.Tasks.Task<IActionResult> OnGetAsync(int id)
        {
            Event = await _db.Events
                .Include(e => e.Organizer)
                .Include(e => e.Tickets)
                .Include(e => e.DiscountPolicies)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (Event == null) return NotFound();

            TicketCount = Event.Tickets.Count;
            IsOpenForReg = Event.Status == OpenStatus;
            DiscountPolicies = Event.DiscountPolicies
                .Where(d => d.Status == "Active").ToList();

            // Thông tin user hiện tại
            var userId = HttpContext.Session.GetInt32("userid");
            IsLoggedIn = userId.HasValue;
            IsParticipant = HttpContext.Session.GetString("role") == "Participant";

            // Kiểm tra đã đăng ký chưa — tìm qua email
            if (IsLoggedIn && IsParticipant)
            {
                var email = HttpContext.Session.GetString("email") ?? "";
                var participant = await _db.Participants
                    .FirstOrDefaultAsync(p => p.Email == email);

                if (participant != null)
                    AlreadyJoined = await _db.Tickets
                        .AnyAsync(t => t.EventId == id
                                    && t.ParticipantId == participant.ParticipantId
                                    && t.Status != "Cancelled");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostRegisterAsync(
            int id, string paymentMethod, string ticketType)
        {
            // ── 1. Phải đăng nhập ─────────────────────────────────────
            var userId = HttpContext.Session.GetInt32("userid");
            if (!userId.HasValue)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đăng ký tham gia sự kiện.";
                return RedirectToPage("/Account/Login",
                    new { returnUrl = $"/Events/EventDetail/{id}" });
            }

            // ── 2. Phải là Participant ─────────────────────────────────
            var role = HttpContext.Session.GetString("role") ?? "";
            if (role != "Participant")
            {
                TempData["Error"] = "Chỉ tài khoản Participant mới có thể đăng ký tham gia sự kiện.";
                return RedirectToPage(new { id });
            }

            // ── 3. Sự kiện phải đang Open ─────────────────────────────
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            if (ev.Status != OpenStatus)
            {
                TempData["Error"] = "Sự kiện này hiện không mở đăng ký.";
                return RedirectToPage(new { id });
            }

            // ── 4. Kiểm tra capacity ──────────────────────────────────
            if (ev.Capacity.HasValue)
            {
                var currentCount = await _db.Tickets
                    .CountAsync(t => t.EventId == id && t.Status != "Cancelled");
                if (currentCount >= ev.Capacity.Value)
                {
                    TempData["Error"] = "Sự kiện đã đủ số lượng người tham gia.";
                    return RedirectToPage(new { id });
                }
            }

            // ── 5. Tìm hoặc tạo Participant ───────────────────────────
            var email = HttpContext.Session.GetString("email") ?? "";
            var fullName = HttpContext.Session.GetString("fullname") ?? "";

            // Fallback: dùng username nếu không có email
            if (string.IsNullOrEmpty(email))
                email = HttpContext.Session.GetString("username") ?? "";

            var participant = await _db.Participants
                .FirstOrDefaultAsync(p => p.Email == email);

            if (participant == null)
            {
                participant = new ParticipantModel 
                {
                    FullName = fullName,
                    Email = email,
                    IsVip = false,
                    IsBlacklisted = false,
                    Status = "Active",
                    UserId = userId
                };
                _db.Participants.Add(participant);
                await _db.SaveChangesAsync();
            }

            // ── 6. Kiểm tra blacklist ─────────────────────────────────
            if (participant.IsBlacklisted == true)
            {
                TempData["Error"] = "Tài khoản của bạn không được phép đăng ký sự kiện.";
                return RedirectToPage(new { id });
            }

            // ── 7. Kiểm tra đã đăng ký chưa ──────────────────────────
            bool alreadyJoined = await _db.Tickets
                .AnyAsync(t => t.EventId == id
                            && t.ParticipantId == participant.ParticipantId
                            && t.Status != "Cancelled");
            if (alreadyJoined)
            {
                TempData["Error"] = "Bạn đã đăng ký sự kiện này rồi.";
                return RedirectToPage(new { id });
            }

            // ── 8. Tính giá theo TicketType ───────────────────────────
            var basePrice = ev.Amount ?? 0;
            var ticketPrice = ticketType switch
            {
                "VIP" => basePrice * 1.5m,
                "Student" => basePrice * 0.7m,
                "Complimentary" => 0m,
                _ => basePrice        // Standard
            };

            // ── 9. Xác định trạng thái thanh toán ────────────────────
            var paymentStatus = paymentMethod switch
            {
                "Direct" => "Pending",   // Trực tiếp → chờ tại quầy
                "Transfer" => "Paid",   // Chuyển khoản → chờ xác nhận
                "CreditCard" => "Paid",      // Thẻ → paid ngay
                "ZaloPay" => "Paid",      // ZaloPay → paid ngay    
                _ => "Pending"
            };

            // ── 10. Tạo Ticket ────────────────────────────────────────
            var ticket = new Ticket
            {
                EventId = id,
                ParticipantId = participant.ParticipantId,
                Qrcode = Guid.NewGuid(),
                TicketType = ticketType,
                Price = ticketPrice,
                PaymentStatus = paymentStatus,
                Status = "Valid"
            };
            _db.Tickets.Add(ticket);

            // ── 11. Ghi audit log ─────────────────────────────────────
            _db.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId = userId,
                Action = $"REGISTER_EVENT_{id}_TYPE_{ticketType}_PAY_{paymentMethod.ToUpper()}",
                TableName = "Tickets",
                ActionTime = DateTime.Now,
                Status = "Success"
            });

            await _db.SaveChangesAsync();

            // ── 12. Thông báo thành công ──────────────────────────────
            var priceDisplay = ticketPrice == 0
                ? "Miễn phí"
                : ticketPrice.ToString("N0") + " VNĐ";

            var msg = paymentMethod switch
            {
                "Direct" => $"Đăng ký thành công! Vé {ticketType} - {priceDisplay}. Vui lòng thanh toán tại quầy vào ngày sự kiện.",
                "Transfer" => $"Đăng ký thành công! Vé {ticketType} - {priceDisplay}. Vui lòng chuyển khoản và chờ xác nhận.",
                "CreditCard" => $"Đăng ký và thanh toán thành công! Vé {ticketType} - {priceDisplay} đã được xác nhận.",
                "ZaloPay" => $"Đăng ký và thanh toán ZaloPay thành công! Vé {ticketType} - {priceDisplay} đã được xác nhận.",
                _ => "Đăng ký thành công!"
            };

            TempData["Success"] = msg;
            return RedirectToPage(new { id });
        }
    }
}