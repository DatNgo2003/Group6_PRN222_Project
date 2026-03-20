using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_PRN222.Helpers;
using Group6_PRN222_Project.Models;

namespace Project_PRN222.Pages.Admin.AuditLogs
{
    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _db;

        public IndexModel(ProjectPrn222Context db) => _db = db;

        public List<SystemAuditLog> Logs { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string? SearchUser    { get; set; }
        [BindProperty(SupportsGet = true)] public string? SearchTable   { get; set; }
        [BindProperty(SupportsGet = true)] public string? SearchAction  { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? DateFrom    { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? DateTo      { get; set; }
        [BindProperty(SupportsGet = true)] public int PageIndex         { get; set; } = 1;

        public int TotalCount  { get; set; }
        public int PageSize    { get; set; } = 20;
        public int TotalPages  => (int)Math.Ceiling((double)TotalCount / PageSize);

        public async Task<IActionResult> OnGetAsync()
        {
#if DEBUG
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                HttpContext.Session.SetInt32("UserID",   1);
                HttpContext.Session.SetString("Role",     "Admin");
                HttpContext.Session.SetString("FullName", "Dev Admin");
                HttpContext.Session.SetString("UserName", "admin");
            }
#else
            if (!SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Auth/Login");
#endif

            var query = _db.SystemAuditLogs
                           .Include(l => l.User)
                           .AsQueryable();

            // Filters
            if (!string.IsNullOrWhiteSpace(SearchUser))
                query = query.Where(l => l.User != null &&
                    (l.User.Username.Contains(SearchUser) ||
                     (l.User.FullName != null && l.User.FullName.Contains(SearchUser))));

            if (!string.IsNullOrWhiteSpace(SearchTable))
                query = query.Where(l => l.TableName != null &&
                    l.TableName.Contains(SearchTable));

            if (!string.IsNullOrWhiteSpace(SearchAction))
                query = query.Where(l => l.Action != null &&
                    l.Action.Contains(SearchAction));

            if (DateFrom.HasValue)
                query = query.Where(l => l.ActionTime >= DateFrom.Value);

            if (DateTo.HasValue)
                query = query.Where(l => l.ActionTime <= DateTo.Value.AddDays(1));

            TotalCount = await query.CountAsync();

            Logs = await query
                .OrderByDescending(l => l.ActionTime)
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }
    }
}
