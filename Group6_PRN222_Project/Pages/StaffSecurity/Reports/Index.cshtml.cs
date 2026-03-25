using Group6_PRN222_Project.Auth;
using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Pages.StaffSecurity.Reports;

[AuthorizeRole("Staff(Security)")]
public class IndexModel : PageModel
{
    private readonly ProjectPrn222Context _db;

    public IndexModel(ProjectPrn222Context db) => _db = db;

    public List<FieldReport> Items { get; set; } = new();

    public async System.Threading.Tasks.Task OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("userid") ?? 0;

        Items = await _db.FieldReports.AsNoTracking()
            .Include(r => r.Event)
            .Where(r => r.StaffId == userId && r.ReportType == "Security")
            .OrderByDescending(r => r.ReportTime)
            .Take(50)
            .ToListAsync();
    }
}
