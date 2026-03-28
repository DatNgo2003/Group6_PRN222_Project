using Task = System.Threading.Tasks.Task;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;

namespace Project_PRN222.Pages.StaffLogistics
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public IndexModel(ProjectPrn222Context db) => _db = db;

        public int ToDoCount { get; set; }
        public int InProgressCount { get; set; }
        public int DoneCount { get; set; }

        public int LogisticsReportsCount { get; set; }
        public int EquipmentReportsCount { get; set; }
        public int IssueReportsCount { get; set; }

        public List<Group6_PRN222_Project.Models.Task> RecentTasks { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                // Debug: tự gán role staff logistics để bạn test nhanh
                HttpContext.Session.SetInt32("UserID", 1);
                HttpContext.Session.SetString("Role", RoleConstants.StaffLogistics);
                HttpContext.Session.SetString("FullName", "Dev Logistics");
                HttpContext.Session.SetString("UserName", "logistics");
            }
#else
            var isLogged = SessionHelper.IsLoggedIn(HttpContext.Session);
            var role = SessionHelper.GetRole(HttpContext.Session);
            if (!isLogged || role != RoleConstants.StaffLogistics)
                return RedirectToPage("/Auth/Login");
#endif

            var actorId = SessionHelper.GetUserID(HttpContext.Session)!.Value;

            var taskQuery = _db.Tasks
                .AsNoTracking()
                .Where(t => t.AssignedTo == actorId);

            var statusCounts = await taskQuery
                .GroupBy(t => t.Status ?? "")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var sc in statusCounts)
            {
                if (string.Equals(sc.Status, "To Do", StringComparison.OrdinalIgnoreCase))
                    ToDoCount = sc.Count;
                else if (string.Equals(sc.Status, "In Progress", StringComparison.OrdinalIgnoreCase))
                    InProgressCount = sc.Count;
                else if (string.Equals(sc.Status, "Done", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(sc.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                    DoneCount = sc.Count;
            }

            RecentTasks = await taskQuery
                .Include(t => t.Event)
                .OrderBy(t => t.Deadline)
                .Take(8)
                .ToListAsync();

            LogisticsReportsCount = await _db.FieldReports
                .AsNoTracking()
                .CountAsync(r => r.StaffId == actorId && r.ReportType == "Logistics");

            EquipmentReportsCount = await _db.FieldReports
                .AsNoTracking()
                .CountAsync(r => r.StaffId == actorId && r.ReportType == "Equipment");

            IssueReportsCount = await _db.FieldReports
                .AsNoTracking()
                .CountAsync(r => r.StaffId == actorId && r.ReportType == "Issue");

            return Page();
        }
    }
}

