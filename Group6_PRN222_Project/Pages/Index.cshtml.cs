using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Group6_PRN222_Project.Models;

// Alias bắt buộc: Models có Task.cs nên "Task" bị ambiguous với System.Threading.Tasks.Task
using AppTask = System.Threading.Tasks.Task;

namespace Group6_PRN222_Project.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public IndexModel(ProjectPrn222Context db)
        {
            _db = db;
        }

        // ── Stats ──────────────────────────────────────────────────────
        public int TotalEvents { get; set; }
        public int TotalParticipants { get; set; }
        public int TotalOrganizers { get; set; }
        public int UpcomingCount { get; set; }

        // ── Danh sách sự kiện sắp diễn ra (tối đa 6) ──────────────────
        public List<Event> UpcomingEvents { get; set; } = new();

        // ── GET ────────────────────────────────────────────────────────
        public async AppTask OnGetAsync()
        {
            var now = DateTime.Now;

            TotalEvents = await _db.Events.CountAsync();

            TotalParticipants = await _db.Tickets.CountAsync();

            TotalOrganizers = await _db.Users
                .Where(u => u.Role != null && u.Role.RoleName == "Organizer")
                .CountAsync();

            UpcomingCount = await _db.Events
                .Where(e => e.StartDate >= now)
                .CountAsync();

            UpcomingEvents = await _db.Events
                .Include(e => e.Organizer)
                .Where(e => e.StartDate >= now)
                .OrderBy(e => e.StartDate)
                .Take(5)
                .ToListAsync();
        }

        // ── POST: Newsletter ───────────────────────────────────────────
        public IActionResult OnPostSubscribe(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Vui lòng nhập email hợp lệ.";
                return RedirectToPage();
            }
            TempData["Success"] = $"Đăng ký thành công! Chúng tôi sẽ gửi thông tin đến {email}.";
            return RedirectToPage();
        }

        // ── POST: Contact ──────────────────────────────────────────────
        public IActionResult OnPostContact(
            string contactName,
            string contactEmail,
            string contactSubject,
            string contactMessage)
        {
            if (string.IsNullOrWhiteSpace(contactName) || string.IsNullOrWhiteSpace(contactEmail))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin liên hệ.";
                return RedirectToPage("#contact");
            }
            TempData["Success"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
            return RedirectToPage();
        }
    }
}
