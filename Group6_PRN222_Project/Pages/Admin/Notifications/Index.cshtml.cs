using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Project_PRN222.Pages.Admin.Notifications
{
    [AuthorizeRole("Admin")]
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;
        public IndexModel(ProjectPrn222Context db) => _db = db;

        // ── List ──────────────────────────────────────────
        public List<Notification> SentNotifications { get; set; } = new();
        public List<User> Organizers { get; set; } = new();

        // ── Filter / paging ───────────────────────────────
        [BindProperty(SupportsGet = true)] public int PageIndex { get; set; } = 1;
        public int PageSize   { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // ── Send form ─────────────────────────────────────
        [BindProperty] public int?   TargetUserID { get; set; }   // null = broadcast to all organizers
        [BindProperty] public string Title        { get; set; } = string.Empty;
        [BindProperty] public string NotificationContent { get; set; } = string.Empty;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage   { get; set; }

        // ─────────────────────────────────────────────────
        public async Task<IActionResult> OnGetAsync()
        {
            EnsureDevSession();
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            await LoadPageDataAsync();
            return Page();
        }

        // ── POST: send notification ───────────────────────
        public async Task<IActionResult> OnPostSendAsync()
        {
            EnsureDevSession();
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(NotificationContent))
            {
                ErrorMessage = "Tiêu đề và nội dung không được để trống.";
                await LoadPageDataAsync();
                return Page();
            }

            var now = DateTime.Now;

            if (TargetUserID.HasValue)
            {
                // Send to one organizer
                _db.Notifications.Add(new Notification
                {
                    UserId    = TargetUserID.Value,
                    Title     = Title.Trim(),
                    Content   = NotificationContent.Trim(),
                    CreatedAt = now,
                    IsRead    = false
                });
            }
            else
            {
                // Broadcast to all organizers (RoleID = 2)
                var organizers = await _db.Users
                    .Where(u => u.RoleId == 2 && u.Status == "Active")
                    .Select(u => u.UserId)
                    .ToListAsync();

                foreach (var uid in organizers)
                {
                    _db.Notifications.Add(new Notification
                    {
                        UserId    = uid,
                        Title     = Title.Trim(),
                        Content   = NotificationContent.Trim(),
                        CreatedAt = now,
                        IsRead    = false
                    });
                }
            }

            await _db.SaveChangesAsync();
            SuccessMessage = TargetUserID.HasValue
                ? "Đã gửi thông báo thành công."
                : "Đã gửi thông báo tới tất cả Organizer.";

            // Clear form
            Title   = string.Empty;
            NotificationContent = string.Empty;
            TargetUserID = null;

            await LoadPageDataAsync();
            return Page();
        }

        // ── POST: delete notification ─────────────────────
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            EnsureDevSession();
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var n = await _db.Notifications.FindAsync(id);
            if (n != null)
            {
                _db.Notifications.Remove(n);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage(new { PageIndex });
        }

        // ─────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadPageDataAsync()
        {
            Organizers = await _db.Users
                .Where(u => u.RoleId == 2 && u.Status == "Active")
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var query = _db.Notifications
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt);

            TotalCount = await query.CountAsync();

            SentNotifications = await query
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        private void EnsureDevSession()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID",   1);
                HttpContext.Session.SetString("Role",     "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
        }
    }
}
