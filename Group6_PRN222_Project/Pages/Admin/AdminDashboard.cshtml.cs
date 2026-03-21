using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Pages.Admin
{
    [AuthorizeRole("Admin")]
    public class AdminDashboardModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public AdminDashboardModel(ProjectPrn222Context db)
        {
            _db = db;
        }

        public string FullName { get; set; } = "";
        public int TotalUsers { get; set; }
        public int TotalEvents { get; set; }
        public int TotalParticipants { get; set; }
        public int TotalRevenue { get; set; }
        public List<SystemAuditLog> RecentLogs { get; set; } = new();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            FullName = HttpContext.Session.GetString("fullname") ?? "Admin";
            TotalUsers = await _db.Users.CountAsync();
            TotalEvents = await _db.Events.CountAsync();
            TotalParticipants = await _db.Participants.CountAsync();
            TotalRevenue = await _db.Tickets.CountAsync(t => t.PaymentStatus == "Paid");

            RecentLogs = await _db.SystemAuditLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.ActionTime)
                .Take(8)
                .ToListAsync();
        }
    }
}
