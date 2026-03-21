using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Pages.Staff
{
    [AuthorizeRole("Staff(Security)", "Staff(MKT)", "Staff(Logistics)")]
    public class StaffDashboardModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public StaffDashboardModel(ProjectPrn222Context db)
        {
            _db = db;
        }

        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
        public int MyTotalTasks { get; set; }
        public int MyDoneTasks { get; set; }
        public int MyPendingTasks { get; set; }
        public int MyReports { get; set; }

        public List<Group6_PRN222_Project.Models.EventTask> MyTasks { get; set; } = new();
        public List<FieldReport> MyFieldReports { get; set; } = new();

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            FullName = HttpContext.Session.GetString("fullname") ?? "Staff";
            Role = HttpContext.Session.GetString("role") ?? "";
            var userId = HttpContext.Session.GetInt32("userid") ?? 0;

            MyTotalTasks = await _db.Tasks.CountAsync(t => t.AssignedTo == userId);
            MyDoneTasks = await _db.Tasks.CountAsync(t => t.AssignedTo == userId && t.Status == "Done");
            MyPendingTasks = await _db.Tasks.CountAsync(t => t.AssignedTo == userId && t.Status == "To Do");
            MyReports = await _db.FieldReports.CountAsync(r => r.StaffId == userId);

            MyTasks = await _db.Tasks
                .Include(t => t.Event)
                .Where(t => t.AssignedTo == userId)
                .OrderBy(t => t.Deadline)
                .Take(8)
                .ToListAsync();

            MyFieldReports = await _db.FieldReports
                .Include(r => r.Event)
                .Where(r => r.StaffId == userId)
                .OrderByDescending(r => r.ReportTime)
                .Take(6)
                .ToListAsync();
        }
    }
}
